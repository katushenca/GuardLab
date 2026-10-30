# Отчёт по SLI, SLO и SLA GuardLab

Для индикаторов используется метрика probe_success,
которую создаёт Blackbox Exporter по результатам проверок.
VMagent собирает эти значения и передаёт их в VictoriaMetrics. 
Для расчётов используется окно последних 30 дней.

### Выбранные SLI и цели SLO

- Доступность API SLI - доля успешных HTTP-проверок API за 30 дней. Цель SLO: не ниже **99,5%**.
- Доступность PostgreSQL. SLI - доля успешных метрик pg up с PostgreSQL за 30 дней. Цель SLO: не ниже 99,5% 
- Доступность IDOR-сценария SLI - доля успешных проверок критического сценария за 30 дней (см. по метрикам). Цель SLO: не ниже 99.5%

SLA приложения - доступность критического IDOR-сценария не ниже 99% за 30 дней

## Стек мониторинга
Внешние проверки выполняет Blackbox Exporter, vmagent собирает метрики и передаёт их в VictoriaMetrics. 
Графики собраны в Grafana.

## Метрики и SLI
В пробах используется blackbox, поэтому могут быть непройденнные пробы, но sli по метрикам приложения оставаться стабильным
### 1. http_requests_received_total

Метрика http_requests_received_total - счётчик HTTP-запросов API с label code - код ответа. 
В SLI успешными считаются запросы без ответа 5xx

SLI API = (1 - число ответов 5xx / число всех полученных запросов) * 100%

PromQL в дашборде:

```
(100 * (1 - ((sum(increase(http_requests_received_total{code=~"5.."}[$window])) or vector(0)) / clamp_min(sum(increase(http_requests_received_total[$window])), 1)))) and (sum(increase(http_requests_received_total[$window])) > 0)
```


SLO API: доля запросов без 5xx не ниже 99,5% за выбранное окно. Линия цели в Grafana - vector(99.5)

### 2. Доступность PostgreSQL

Метрика pg_up отдаётся PostgreSQL Exporter. Значение 1 - exporter подключился к постгре, 0 - что подключение не удалось.

SLI PostgreSQL = среднее значение pg_up за окно * 100%

PromQL в дашборде:

```
avg_over_time(pg_up[$window]) * 100
```

SLO Postgresql: доступность не ниже 99,5% за выбранное окно. Линия цели строится запросом vector(99.5)

Если 

### 3. Корректность отказа защищённой IDOR-реализации

Метрика guardlab_lab_key_access_total считает результаты проверок доступа к учебному ключу. 
 Успешной проверкой считается ожидаемый отказ result="denied"

```
SLI IDOR = число ожидаемых отказов / число всех проверок safe-реализации * 100%
```

```
(100 * ((sum(increase(guardlab_lab_key_access_total{source="synthetic",implementation="safe",result="denied"}[$window])) or vector(0)) / clamp_min(sum(increase(guardlab_lab_key_access_total{source="synthetic",implementation="safe"}[$window])), 1))) and (sum(increase(guardlab_lab_key_access_total{source="synthetic",implementation="safe"}[$window])) > 0)
```

Есть отдельная панель для учитывания запросов пользователей:
```
100 * sum(increase(guardlab_lab_key_access_total{source="user",implementation="safe",result="denied"}[$window]))
/
sum(increase(guardlab_lab_key_access_total{source="user",implementation="safe"}[$window]))
```
SLO защищённой реализации: все синтетические проверки должны получить ожидаемый отказ, целевое значение 100%. 

## SLA приложения

Для SLA используется Blackbox-проба guardlab-probe-idor-scenario.
Она выполняет основной сценарий через апи и проверяет ожидаемые результаты уязвимой и защищённой реализаций

```
SLI сценария = среднее значение probe_success за окно * 100%
```

```promql
avg_over_time(probe_success{job="guardlab-probe-idor-scenario"}[$window]) * 100
```

SLA приложения: доступность критического IDOR-сценария не ниже 99% за 30 дней

При цели SLA 99% допустимая доля неуспешных проверок составляет 1%. Остаток бюджета ошибок рассчитывается так:

```
Остаток бюджета = (1 - доля неуспешных проб / 0,01) * 100%
```

```
(1 - ((1 - avg_over_time(probe_success{job="guardlab-probe-idor-scenario"}[$window])) / 0.01)) * 100
```

## Пробы и проверка

Blackbox Exporter проверяет HTTP-доступность API, TCP-подключение к PostgreSQL и выполнение IDOR-сценария через API. 
Метрика probe_success показывает результат каждой внешней проверки. О

В Grafana панели показывают текущее состояние проб, 
5 SLI с целями SLO, выполнение SLA по IDOR-сценарию и остаток бюджета ошибок

## Описание изменений приложения для скринкаста
Для того чтобы умышленно слломать приложение, выставила 0 реплик для апи:
kubectl scale deployment/guardlab-api -n guardlab --replicas=0

на скринкасте как раз таки показала важность blackbox - даже при отсутсвтии реплик (а соотв. и метрик) дашборды,
настроенные на whitebox вели себя нормально, как будто ничего не произошло, blackbox (пробы) же показали реальные проблемы с
приложением

После того как вернула кол-во реплик sli начал стабилизироваться.