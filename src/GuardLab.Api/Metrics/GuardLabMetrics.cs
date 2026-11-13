using Prometheus;

namespace GuardLab.Api.Metrics;

public static class GuardLabMetrics
{
    // Считает попытки запуска учебных сценариев
    private static readonly Counter LabRuns = Prometheus.Metrics.CreateCounter(
        "guardlab_lab_runs_total",
        "Number of GuardLab scenario run attempts.",
        labelNames: new[] { "result", "source" });

    // Сравнивает результат проверки ключа у уязвимой и защищённой реализаций
    // Полезно, чтобы по мониторингу понимать, что с задумкой всё ок и защищенная реализация реально работает
    private static readonly Counter KeyAccesses = Prometheus.Metrics.CreateCounter(
        "guardlab_lab_key_access_total",
        "Number of key access checks performed by each lab implementation.",
        labelNames: new[] { "implementation", "result", "source" });

    // Показывает число успешных и неуспешных попыток входа администратора по результату
    private static readonly Counter LoginAttempts = Prometheus.Metrics.CreateCounter(
        "guardlab_auth_login_attempts_total",
        "Number of administrator login attempts.",
        labelNames: new[] { "result" });

    // Считает успешно выполненные действия администратора над лабораториями
    private static readonly Counter LabLifecycleActions = Prometheus.Metrics.CreateCounter(
        "guardlab_lab_lifecycle_actions_total",
        "Number of successful laboratory lifecycle actions.",
        labelNames: new[] { "action" });

    private static readonly Histogram HttpRequestSize = Prometheus.Metrics.CreateHistogram(
        "http_request_size_bytes",
        "Declared HTTP request body size in bytes from Content-Length.",
        new HistogramConfiguration { Buckets = Histogram.ExponentialBuckets(128, 2, 12) });

    private static readonly Histogram HttpResponseSize = Prometheus.Metrics.CreateHistogram(
        "http_response_size_bytes",
        "Declared HTTP response body size in bytes from Content-Length.",
        new HistogramConfiguration { Buckets = Histogram.ExponentialBuckets(128, 2, 12) });

    public static void ObserveHttpRequestSize(long bytes) => HttpRequestSize.Observe(Math.Max(0, bytes));

    public static void ObserveHttpResponseSize(long bytes) => HttpResponseSize.Observe(Math.Max(0, bytes));
    public static void RecordLabRun(string result) => RecordLabRun(result, "user");

    public static void RecordLabRun(string result, string source) => LabRuns.WithLabels(result, source).Inc();

    public static void RecordKeyAccess(string implementation, int statusCode, string source = "user")
    {
        var result = statusCode switch
        {
            200 => "allowed",
            403 => "denied",
            _ => "not_found"
        };

        KeyAccesses.WithLabels(implementation, result, source).Inc();
    }

    public static void RecordLoginAttempt(string result) => LoginAttempts.WithLabels(result).Inc();

    public static void RecordLabLifecycleAction(string action) => LabLifecycleActions.WithLabels(action).Inc();
}
