-- Add the stable synthetic identifiers to the IDOR exercise description.
SET ROLE db_owner;

UPDATE platform.labs
SET theory = theory || E'\n\nTraining identifiers:\nsid_alice_demo -> accountId 20000000-0000-4000-8000-000000000001\nsid_bob_demo -> accountId 20000000-0000-4000-8000-000000000002\nAlice keyId: 30000000-0000-4000-8000-000000000001\nBob keyId: 30000000-0000-4000-8000-000000000002'
WHERE slug = 'idor-key-access'
  AND theory NOT LIKE '%Training identifiers:%';

RESET ROLE;
