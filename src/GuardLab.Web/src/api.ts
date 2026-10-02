export type Lab = { id: string; slug: string; title: string; summary: string; theory: string; status: number };
export type LabRun = {
    vulnerableStatusCode: number;
    safeStatusCode: number;
    vulnerableResponse: unknown;
    safeResponse: unknown;
    explanation: string
};
const bases = window.location.hostname === 'localhost' && window.location.port === '8080' ? ['http://localhost:5080/api', 'http://localhost:5000/api'] : ['/api'];

export async function api<T>(path: string, options: RequestInit = {}): Promise<T> {
    const token = sessionStorage.getItem('guardlab_admin_token');
    const headers = {...(options.body ? {'Content-Type': 'application/json'} : {}), ...(token ? {Authorization: `Bearer ${token}`} : {}), ...(options.headers ?? {})};
    let last: unknown;
    for (const base of bases) {
        try {
            const r = await fetch(`${base}${path}`, {...options, headers});
            const body = await r.json().catch(() => null);
            if (!r.ok) throw new Error(body?.message ?? body?.error ?? `HTTP ${r.status}`);
            return body as T
        } catch (e) {
            last = e
        }
    }
    throw last instanceof Error ? last : new Error('Не удалось связаться с API')
}
