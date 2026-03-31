# Wizard-RPG — GitOps Deployment Guide

This guide walks you through deploying the **Wizard-RPG** application on a single-node **k3s** cluster managed by **Rancher**, running on **Proxmox VE**. The stack uses **ArgoCD** for GitOps-based continuous delivery and **CloudNativePG** for PostgreSQL.

---

## Table of Contents

1. [Architecture Overview](#architecture-overview)
2. [Prerequisites](#prerequisites)
3. [Infrastructure Setup](#infrastructure-setup)
4. [Installing ArgoCD](#installing-argocd)
5. [Installing CloudNativePG Operator](#installing-cloudnativepg-operator)
6. [Deploying the Application](#deploying-the-application)
7. [Accessing the Application](#accessing-the-application)
8. [Managing Secrets](#managing-secrets)
9. [Useful Commands](#useful-commands)
10. [Troubleshooting](#troubleshooting)
11. [Directory Structure](#directory-structure)

---

## Architecture Overview

```
┌──────────────────────────────────────────────────────┐
│                    Proxmox VE Host                   │
│  ┌────────────────────────────────────────────────┐  │
│  │              VM (Ubuntu / Debian)               │  │
│  │  ┌──────────────────────────────────────────┐  │  │
│  │  │              k3s (single node)            │  │  │
│  │  │                                          │  │  │
│  │  │  ┌─────────┐  ┌──────────────────────┐  │  │  │
│  │  │  │ ArgoCD  │  │     Traefik Ingress  │  │  │  │
│  │  │  └────┬────┘  └──────────┬───────────┘  │  │  │
│  │  │       │                  │               │  │  │
│  │  │       ▼                  ▼               │  │  │
│  │  │  ┌─────────────────────────────────┐    │  │  │
│  │  │  │      wizard-rpg namespace       │    │  │  │
│  │  │  │  ┌────────────┐ ┌────────────┐  │    │  │  │
│  │  │  │  │ Wizard-RPG │ │ PostgreSQL │  │    │  │  │
│  │  │  │  │   (App)    │ │ (CNPG)     │  │    │  │  │
│  │  │  │  └────────────┘ └────────────┘  │    │  │  │
│  │  │  └─────────────────────────────────┘    │  │  │
│  │  └──────────────────────────────────────────┘  │  │
│  └────────────────────────────────────────────────┘  │
└──────────────────────────────────────────────────────┘
```

**Key components:**

| Component | Purpose |
|---|---|
| **k3s** | Lightweight Kubernetes distribution (includes Traefik) |
| **Rancher** | Kubernetes management UI (optional) |
| **ArgoCD** | GitOps continuous delivery — watches this repo |
| **CloudNativePG** | Kubernetes-native PostgreSQL operator |
| **Traefik** | Ingress controller (bundled with k3s) |

---

## Prerequisites

- A **Proxmox VE** host with a VM running Ubuntu 22.04+ or Debian 12+
- VM specs (minimum): 2 vCPUs, 4 GB RAM, 30 GB disk
- SSH access to the VM
- `kubectl` installed on your local machine
- `git` installed on your local machine
- A DNS entry or `/etc/hosts` entry pointing `wizard-rpg.local` and `argocd.local` to your VM's IP

---

## Infrastructure Setup

### 1. Install k3s

SSH into your Proxmox VM and run:

```bash
curl -sfL https://get.k3s.io | sh -
```

Verify the installation:

```bash
sudo k3s kubectl get nodes
```

Copy the kubeconfig for local access:

```bash
# On the VM
sudo cat /etc/rancher/k3s/k3s.yaml
```

On your local machine, save the output to `~/.kube/config` and replace `127.0.0.1` with your VM's IP address.

```bash
export KUBECONFIG=~/.kube/config
kubectl get nodes
```

### 2. Install Rancher (Optional)

If you want a web UI for managing your cluster:

```bash
# Add Rancher Helm repo
helm repo add rancher-latest https://releases.rancher.com/server-charts/latest
helm repo update

# Install cert-manager (required by Rancher)
kubectl apply -f https://github.com/cert-manager/cert-manager/releases/download/v1.14.4/cert-manager.yaml

# Wait for cert-manager to be ready
kubectl wait --for=condition=Available deployment --all -n cert-manager --timeout=120s

# Install Rancher
helm install rancher rancher-latest/rancher \
  --namespace cattle-system --create-namespace \
  --set hostname=rancher.local \
  --set replicas=1 \
  --set bootstrapPassword=admin
```

---

## Installing ArgoCD

### Option A: Using the k3s Helm Controller (Recommended)

Apply the HelmChart manifest that leverages the k3s built-in Helm controller:

```bash
sudo kubectl apply -f gitops/infrastructure/argocd/helmchart.yaml
```

### Option B: Manual Install

```bash
kubectl create namespace argocd
kubectl apply -n argocd \
  -f https://raw.githubusercontent.com/argoproj/argo-cd/stable/manifests/install.yaml
```

### Get the ArgoCD Admin Password

```bash
kubectl -n argocd get secret argocd-initial-admin-secret \
  -o jsonpath="{.data.password}" | base64 -d; echo
```

### Access the ArgoCD UI

If you used Option A, ArgoCD is accessible via the Traefik ingress at `https://argocd.local`.

Alternatively, use port-forwarding:

```bash
kubectl port-forward svc/argocd-server -n argocd 8443:443
```

Then open https://localhost:8443 in your browser.

- **Username:** `admin`
- **Password:** *(from the command above)*

### Install the ArgoCD CLI (Optional)

```bash
# Linux
curl -sSL -o argocd https://github.com/argoproj/argo-cd/releases/latest/download/argocd-linux-amd64
chmod +x argocd
sudo mv argocd /usr/local/bin/

# Login
argocd login localhost:8443 --insecure --username admin --password <password>
```

---

## Installing CloudNativePG Operator

### Option A: Using the k3s Helm Controller (Recommended)

```bash
sudo kubectl apply -f gitops/infrastructure/cnpg-operator/helmchart.yaml
```

### Option B: Using Helm Directly

```bash
helm repo add cnpg https://cloudnative-pg.github.io/charts
helm repo update
helm upgrade --install cnpg cnpg/cloudnative-pg \
  --namespace cnpg-system --create-namespace
```

### Verify the Operator Is Running

```bash
kubectl get pods -n cnpg-system
```

You should see the `cnpg-cloudnative-pg` controller pod in a `Running` state.

---

## Deploying the Application

### Step 1: Register the ArgoCD Project and Applications

```bash
kubectl apply -f gitops/argocd/project.yaml
kubectl apply -f gitops/argocd/applications.yaml
```

This registers three ArgoCD applications (deployed in order via sync-waves):

1. **cnpg-operator** — installs the CloudNativePG operator
2. **wizard-rpg-database** — creates the PostgreSQL cluster
3. **wizard-rpg** — deploys the application

### Step 2: Watch the Deployment

In the ArgoCD UI or via CLI:

```bash
# Check application status
argocd app list

# Watch sync status
argocd app get wizard-rpg
argocd app get wizard-rpg-database
```

Or using kubectl:

```bash
# Check all resources in the wizard-rpg namespace
kubectl get all -n wizard-rpg

# Check the PostgreSQL cluster status
kubectl get cluster -n wizard-rpg

# Check the database pods
kubectl get pods -n wizard-rpg -l cnpg.io/cluster=wizard-rpg-db
```

### Step 3: Verify the Database Is Ready

```bash
kubectl get cluster wizard-rpg-db -n wizard-rpg
```

The `STATUS` should show `Cluster in healthy state` once the database is ready.

CloudNativePG automatically creates several secrets:

| Secret Name | Contents |
|---|---|
| `wizard-rpg-db-app` | Application user credentials (`username`, `password`, `uri`) |
| `wizard-rpg-db-superuser` | Superuser credentials |

The application deployment references `wizard-rpg-db-app` to connect to the database.

---

## Accessing the Application

### DNS / Hosts Setup

Add the following to your local `/etc/hosts` file (replace `<VM_IP>` with your Proxmox VM's IP):

```
<VM_IP>  wizard-rpg.local
<VM_IP>  argocd.local
```

Then open http://wizard-rpg.local in your browser.

### Using Port-Forward (Alternative)

```bash
kubectl port-forward svc/wizard-rpg -n wizard-rpg 8080:80
```

Then open http://localhost:8080 in your browser.

---

## Managing Secrets

The `secret.yaml` in `gitops/apps/wizard-rpg/` is a **template** with placeholder values. For production use, consider one of these approaches:

### Option 1: SealedSecrets (Recommended)

```bash
# Install SealedSecrets controller
helm repo add sealed-secrets https://bitnami-labs.github.io/sealed-secrets
helm install sealed-secrets sealed-secrets/sealed-secrets -n kube-system

# Install kubeseal CLI
# Then seal your secrets:
kubeseal --format yaml < my-secret.yaml > sealed-secret.yaml
```

### Option 2: External Secrets Operator

Use the [External Secrets Operator](https://external-secrets.io/) to pull secrets from a vault (HashiCorp Vault, AWS Secrets Manager, etc.).

### Option 3: Manual Secret Creation

Create the secret directly on the cluster (not stored in Git):

```bash
kubectl create secret generic wizard-rpg-secret \
  --namespace wizard-rpg \
  --from-literal=APP_SECRET_KEY="your-actual-secret-key"
```

> **Note:** Database credentials are managed automatically by CloudNativePG. The operator creates the `wizard-rpg-db-app` secret when the Cluster resource is created.

---

## Useful Commands

```bash
# --- Cluster ---
kubectl get nodes                              # List nodes
kubectl top nodes                              # Node resource usage

# --- Application ---
kubectl get all -n wizard-rpg                  # All resources
kubectl logs -f deploy/wizard-rpg -n wizard-rpg  # Application logs
kubectl describe deploy wizard-rpg -n wizard-rpg  # Deployment details

# --- Database ---
kubectl get cluster -n wizard-rpg              # CNPG cluster status
kubectl get pods -n wizard-rpg -l cnpg.io/cluster=wizard-rpg-db  # DB pods
kubectl logs wizard-rpg-db-1 -n wizard-rpg     # Database logs

# Connect to the database
kubectl exec -it wizard-rpg-db-1 -n wizard-rpg -- psql -U wizard_rpg -d wizard_rpg

# --- ArgoCD ---
argocd app list                                # List all apps
argocd app sync wizard-rpg                     # Force sync
argocd app sync wizard-rpg-database            # Force DB sync

# --- Ingress ---
kubectl get ingress -n wizard-rpg              # List ingress rules
kubectl get svc -n kube-system                 # Traefik service info
```

---

## Troubleshooting

### Application Pod Is CrashLooping

```bash
# Check logs
kubectl logs -f deploy/wizard-rpg -n wizard-rpg

# Check events
kubectl describe pod -l app.kubernetes.io/name=wizard-rpg -n wizard-rpg
```

Common causes:
- The Docker image hasn't been pushed yet (check `gitops/apps/wizard-rpg/deployment.yaml` for the image reference)
- Database is not ready yet — check that the CNPG cluster is healthy
- Missing environment variables or secrets

### Database Not Starting

```bash
# Check CNPG operator logs
kubectl logs -f deploy/cnpg-cloudnative-pg -n cnpg-system

# Check cluster status
kubectl describe cluster wizard-rpg-db -n wizard-rpg
```

Common causes:
- CNPG operator is not installed
- Insufficient disk space (check storage class and PVCs)
- Storage class `local-path` not available

### ArgoCD Sync Failures

```bash
# Check application status
argocd app get wizard-rpg

# Check for sync errors
argocd app get wizard-rpg --show-operation
```

Common causes:
- Repository not accessible from the cluster
- CRDs not installed (CNPG operator must be installed before database resources)
- RBAC issues

### Ingress Not Working

```bash
# Check Traefik is running
kubectl get pods -n kube-system -l app.kubernetes.io/name=traefik

# Check ingress resource
kubectl describe ingress wizard-rpg -n wizard-rpg
```

Common causes:
- DNS/hosts file not configured
- Traefik not running
- Wrong ingress class annotation

---

## Directory Structure

```
gitops/
├── README.md                                    # This guide
├── argocd/
│   ├── project.yaml                             # ArgoCD project definition
│   └── applications.yaml                        # ArgoCD application definitions
├── apps/
│   └── wizard-rpg/
│       ├── kustomization.yaml                   # Kustomize entry point
│       ├── namespace.yaml                       # Namespace definition
│       ├── configmap.yaml                       # Application configuration
│       ├── secret.yaml                          # Secret template (placeholder values)
│       ├── deployment.yaml                      # Application deployment
│       ├── service.yaml                         # ClusterIP service
│       └── ingress.yaml                         # Traefik ingress rule
├── database/
│   ├── kustomization.yaml                       # Kustomize entry point
│   └── cluster.yaml                             # CloudNativePG PostgreSQL cluster
└── infrastructure/
    ├── argocd/
    │   ├── kustomization.yaml                   # Kustomize entry point
    │   └── helmchart.yaml                       # ArgoCD Helm install (k3s)
    └── cnpg-operator/
        ├── kustomization.yaml                   # Kustomize entry point
        └── helmchart.yaml                       # CNPG operator Helm install (k3s)
```

---

## What's Next

- [ ] Push the Docker image once the Dockerfile PR is merged
- [ ] Update `deployment.yaml` with the correct image tag
- [ ] Configure TLS/HTTPS via cert-manager (uncomment TLS sections in `ingress.yaml`)
- [ ] Set up proper secret management (SealedSecrets or External Secrets)
- [ ] Enable PostgreSQL backups to object storage (S3/MinIO)
- [ ] Add monitoring with Prometheus + Grafana
