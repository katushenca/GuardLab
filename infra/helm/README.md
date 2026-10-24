# VictoriaMetrics в Minikube

Для хранения метрик используется VictoriaMetrics Single, а vmagent обнаруживает поды в guardlab ns
 Настройки - victoria-metrics-values.yaml.

## Запуск

1. ```
   minikube status
   kubectl config current-context
   ```

2. ```
   kubectl apply -f infra/manifests
   kubectl rollout status deployment/guardlab-api -n guardlab
   ```

3. Дальше надо добавить Helm-репозиторий VictoriaMetrics
   ```
   helm repo add vm https://victoriametrics.github.io/helm-charts
   helm repo update
   helm upgrade --install vm-single vm/victoria-metrics-k8s-stack `
     --namespace monitoring `
     --create-namespace `
     --version 0.96.0 `
     --values infra/helm/victoria-metrics-values.yaml `
     --wait `
     --timeout 10m
   ```
4. ```
   kubectl get pods -n monitoring
   kubectl get svc -n monitoring
   kubectl get pods -n guardlab
   ```

## Проверка метрик
```
kubectl port-forward -n monitoring svc/<имя-сервиса-vmsingle> 8428:8428
```
можно посмотреть, что пишется в vmsignle (точнее какие метрики в ней уже хранятся)
http://localhost:8428/api/v1/label/__name__/values

kubectl port-forward -n monitoring svc/<имя-сервиса-vmagent> 8429:8429
http://localhost:8429/metrics - метрики агента


## Экспорт метрик PostgreSQL

Для сбора метрик PostgreSQL используется postgres-exporter.
Он запускается вторым контейнером в Pod базы и отдаёт метрики на порту 9187.
Пароль хранится отдельно в infra/manifests/postgres-exporter.env
```
Copy-Item infra/manifests/postgres-exporter.env.example infra/manifests/postgres-exporter.env
notepad infra/manifests/postgres-exporter.env
```

```
kubectl create secret generic postgres-exporter-credentials `
  --namespace guardlab `
  --from-env-file=infra/manifests/postgres-exporter.env `
  --dry-run=client -o yaml | kubectl apply -f -
kubectl apply -f infra/manifests/3-pg-configmap.yaml
kubectl apply -f infra/manifests/4-pg-statefulset.yaml
kubectl apply -f infra/manifests/5a-postgres-exporter-user-job.yaml
kubectl rollout status statefulset/postgres -n guardlab
kubectl wait --for=condition=complete job/postgres-exporter-user-setup -n guardlab --timeout=180s
kubectl get pods -n guardlab
```

Для проверки метрик нужно пробросить порт exporter:
```
kubectl port-forward -n guardlab pod/postgres-0 9187:9187
```

и можно открыть /metrics:
http://localhost:9187/metrics

