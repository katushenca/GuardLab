-- Repeatable training data seed. Apply after platform and sandbox schema creation.
SET ROLE db_owner;

INSERT INTO platform.labs (id, slug, title, summary, theory, status)
VALUES ('10000000-0000-4000-8000-000000000001', 'idor-key-access', 'Доступ к чужому API-ключу', 'Сравните выдачу ключа без проверки владельца и с проверкой владельца. Используйте sid из описания задания.', 'Учебные sid sid_alice_demo и sid_bob_demo принадлежат Alice и Bob. Идентификатор объекта не подтверждает право доступа: сервер должен сопоставить владельца ключа с аккаунтом текущей сессии.', 1)
ON CONFLICT (id) DO UPDATE SET
    slug = EXCLUDED.slug,
    title = EXCLUDED.title,
    summary = EXCLUDED.summary,
    theory = EXCLUDED.theory,
    status = EXCLUDED.status;

INSERT INTO sandbox.accounts (id, alias)
VALUES ('20000000-0000-4000-8000-000000000001', 'alice'), ('20000000-0000-4000-8000-000000000002', 'bob')
ON CONFLICT (id) DO NOTHING;

INSERT INTO sandbox.api_keys (id, account_id, display_name, masked_value)
VALUES ('30000000-0000-4000-8000-000000000001', '20000000-0000-4000-8000-000000000001', 'Alice staging key', 'gl_train_****1001'), ('30000000-0000-4000-8000-000000000002', '20000000-0000-4000-8000-000000000002', 'Bob reporting key', 'gl_train_****2002')
ON CONFLICT (id) DO NOTHING;

RESET ROLE;
