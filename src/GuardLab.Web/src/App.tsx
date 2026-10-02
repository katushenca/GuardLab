import {FormEvent, useEffect, useState, type ReactNode} from 'react';
import {Link, Navigate, Route, Routes, useNavigate, useParams} from 'react-router-dom';
import {api, Lab, LabRun} from './api';

const bobKey = '30000000-0000-4000-8000-000000000002';

function Layout({children, admin = false}: { children: ReactNode; admin?: boolean }) {
    return <>
        <header><strong>◇
            GuardLab</strong><span>{admin ? 'административная зона' : 'учебная среда веб-безопасности'}</span>
            <nav>{admin ? <Link to="/">← К каталогу</Link> : <Link to="/admin-login">Вход администратора</Link>}</nav>
        </header>
        <main>{children}</main>
    </>
}

function Notice({children, error = false}: { children: ReactNode; error?: boolean }) {
    return <div className={`notice ${error ? 'error' : ''}`}>{children}</div>
}

function Catalog() {
    const [labs, setLabs] = useState<Lab[]>([]);
    const [error, setError] = useState('');
    useEffect(() => {
        api<Lab[]>('/labs').then(setLabs).catch(e => setError(e.message))
    }, []);
    return <Layout>
        <section className="hero"><small>ЛАБОРАТОРИИ</small><h1>Исследуйте уязвимость безопасно</h1><p>Сравнивайте
            уязвимые и защищённые реализации на синтетических данных.</p></section>
        {error && <Notice error>{error}</Notice>}
        <div className="cards">{labs.map(l => <Link className="lab-card" to={`/labs/${l.slug}`} key={l.id}>
            <h2>{l.title}</h2><p>{l.summary}</p></Link>)}</div>
        {!labs.length && !error && <p>Загрузка каталога…</p>}</Layout>
}

function LabPage() {
    const {slug = ''} = useParams();
    const [lab, setLab] = useState<Lab>();
    const [error, setError] = useState('');
    useEffect(() => {
        api<Lab>(`/labs/${encodeURIComponent(slug)}`).then(setLab).catch(e => setError(e.message))
    }, [slug]);
    if (error) return <Layout><Notice error>{error}</Notice></Layout>;
    if (!lab) return <Layout><p>Загрузка…</p></Layout>;
    return <Layout><small>{lab.slug}</small><h1>{lab.title}</h1><p>{lab.summary}</p>
        <div className="theory">{lab.theory}</div>
        {slug === 'idor-key-access' ? <IdorForm lab={lab}/> :
            <Notice>Для этой лаборатории учебный обработчик ещё не подключён.</Notice>}</Layout>
}

function IdorForm({lab, preview = false}: { lab: Lab; preview?: boolean }) {
    const [sid, setSid] = useState('sid_alice_demo');
    const [keyId, setKeyId] = useState(bobKey);
    const [result, setResult] = useState<LabRun>();
    const [error, setError] = useState('');

    async function submit(e: FormEvent) {
        e.preventDefault();
        try {
            setResult(await api<LabRun>(preview ? `/admin/labs/${lab.id}/run` : `/labs/${lab.slug}/run`, {
                method: 'POST',
                body: JSON.stringify({sid, keyId})
            }))
        } catch (e) {
            setError((e as Error).message)
        }
    }

    return <section><Notice>Sandbox: только синтетические Alice/Bob. Собственный ключ ожидаемо даёт safe 200, чужой —
        safe 403.</Notice>
        <form onSubmit={submit} className="form"><label>sid<input value={sid}
                                                                  onChange={e => setSid(e.target.value)}/></label><label>keyId<input
            value={keyId} onChange={e => setKeyId(e.target.value)}/></label>
            <button>{preview ? 'Запустить preview' : 'Запустить сравнение'}</button>
        </form>
        {error && <Notice error>{error}</Notice>}{result && <div className="results">
            <pre>Уязвимая версия · HTTP {result.vulnerableStatusCode}{`\n`}{JSON.stringify(result.vulnerableResponse, null, 2)}</pre>
            <pre>Защищённая версия · HTTP {result.safeStatusCode}{`\n`}{JSON.stringify(result.safeResponse, null, 2)}</pre>
            <Notice>{result.explanation}</Notice></div>}</section>
}

function Login() {
    const nav = useNavigate();
    const [error, setError] = useState('');

    async function submit(e: FormEvent<HTMLFormElement>) {
        e.preventDefault();
        try {
            const r = await api<{ accessToken: string }>('/auth/login', {
                method: 'POST',
                body: JSON.stringify(Object.fromEntries(new FormData(e.currentTarget)))
            });
            sessionStorage.setItem('guardlab_admin_token', r.accessToken);
            nav('/admin')
        } catch (e) {
            setError((e as Error).message)
        }
    }

    return <Layout>
        <div className="auth card"><small>АДМИНИСТРАТИВНАЯ ЗОНА</small><h1>Вход администратора</h1>
            <form onSubmit={submit} className="form"><label>Email<input name="email" type="email" required
                                                                        defaultValue="admin@example.local"/></label><label>Пароль<input
                name="password" type="password" required/></label>
                <button>Войти</button>
            </form>
            {error && <Notice error>{error}</Notice>}</div>
    </Layout>
}

function Editor({lab, onSubmit}: { lab?: Lab; onSubmit: (e: FormEvent<HTMLFormElement>) => void }) {
    return <form onSubmit={onSubmit} className="form"><input name="id" type="hidden" defaultValue={lab?.id}/><label>Slug<input
        name="slug" required defaultValue={lab?.slug}/></label><label>Название<input name="title" required
                                                                                     defaultValue={lab?.title}/></label><label>Summary<textarea
        name="summary" required defaultValue={lab?.summary}/></label><label>Theory<textarea name="theory" required
                                                                                            defaultValue={lab?.theory}/></label><label>Статус<select
        name="status" defaultValue={lab?.status ?? 0}>
        <option value="0">Draft</option>
        <option value="1">Published</option>
        <option value="2">Archived</option>
    </select></label>
        <button>Сохранить</button>
    </form>
}

function Admin() {
    const nav = useNavigate();
    const [labs, setLabs] = useState<Lab[]>([]);
    const [selected, setSelected] = useState<Lab>();
    const [error, setError] = useState('');
    const load = () => api<Lab[]>('/admin/labs').then(setLabs).catch(e => setError(e.message));
    useEffect(() => {
        if (!sessionStorage.getItem('guardlab_admin_token')) nav('/admin-login'); else load()
    }, []);
    if (!sessionStorage.getItem('guardlab_admin_token')) return null;

    async function save(e: FormEvent<HTMLFormElement>) {
        e.preventDefault();
        const d = Object.fromEntries(new FormData(e.currentTarget)) as Record<string, any>;
        const id = d.id as string;
        delete d.id;
        d.status = Number(d.status);
        try {
            await api(id ? `/admin/labs/${id}` : '/admin/labs', {method: id ? 'PUT' : 'POST', body: JSON.stringify(d)});
            await load();
            setSelected(undefined)
        } catch (e) {
            setError((e as Error).message)
        }
    }

    async function action(l: Lab, a: 'publish' | 'archive' | 'restore') {
        try {
            if (a === 'archive') await api(`/admin/labs/${l.id}/archive`, {method: 'POST'}); else if (a === 'restore') await api(`/admin/labs/${l.id}/restore`, {method: 'POST'}); else await api(`/admin/labs/${l.id}`, {
                method: 'PUT',
                body: JSON.stringify({...l, status: 1})
            });
            await load()
        } catch (e) {
            setError((e as Error).message)
        }
    }

    return <Layout admin><h1>Управление лабораториями</h1>{error && <Notice error>{error}</Notice>}
        <section className="card"><h2>Все лаборатории</h2>{labs.map(l => <div className="admin-row" key={l.id}>
            <span><strong>{l.title}</strong><br/><small>{l.slug} · {['Draft', 'Published', 'Archived'][l.status]}</small></span><span><button
            className="secondary" onClick={() => setSelected(l)}>Изменить</button>
            {l.status === 0 && <button onClick={() => action(l, 'publish')}>Опубликовать</button>}{l.status === 1 &&
                <button className="secondary"
                        onClick={() => action(l, 'archive')}>Архивировать</button>}{l.status === 2 &&
                <button className="secondary" onClick={() => action(l, 'restore')}>Восстановить</button>}</span></div>)}
        </section>
        <section className="card"><h2>{selected ? 'Редактирование' : 'Новая лаборатория'}</h2><Editor lab={selected}
                                                                                                      onSubmit={save}/>
            <button className="secondary" onClick={() => setSelected(undefined)}>Очистить</button>
        </section>
        {selected?.slug === 'idor-key-access' &&
            <section className="card"><h2>Preview sandbox</h2><IdorForm lab={selected} preview/></section>}
        <button className="secondary" onClick={() => {
            sessionStorage.removeItem('guardlab_admin_token');
            nav('/admin-login')
        }}>Выйти
        </button>
    </Layout>
}

export default function App() {
    return <Routes><Route path="/" element={<Catalog/>}/><Route path="/labs/:slug" element={<LabPage/>}/><Route
        path="/admin-login" element={<Login/>}/><Route path="/admin-login.html" element={<Login/>}/><Route path="/admin"
                                                                                                           element={
                                                                                                               <Admin/>}/><Route
        path="/admin.html" element={<Admin/>}/><Route path="*" element={<Navigate to="/" replace/>}/></Routes>
}
