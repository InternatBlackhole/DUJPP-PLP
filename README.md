# Navodila za uporabo

Za pogon projekta potrebujete Docker in Docker Compose.
Potrebno je definirati tudi datoteko `.env` v istem imeniku kot datoteka `docker-compose.yaml`.

```env
POSTGRES_ROOT_USER=root
POSTGRES_ROOT_PWD=12345678abcdef
HTTP_HOST="127.0.0.1:5000"
HTTPS_HOST="127.0.0.1:5443"
```

Zgornji primer lahko uporabite ali prilagodite.

Za zagon poženite `docker compose up -d` in počakajte, da se vse izvede.

**Če želite produkcijsko okolje morate pognati `docker compose -f docker-compose.prod.yaml up -d` in vedno uporabiti `-f docker-compose.prod.yaml`**

Aplikacija je dostopna na naslovu podanem v okoljski spremenljivki `HTTP_HOST` ali `HTTPS_HOST` (https nima veljavnega certifikata, tako da bo brskalnik malo tečen).
Predlagam uporabo http variante.

V aplikacijo se lahko prijavite z naslednjimi uporabniki
|         Email        |    Geslo    |   Vloga   |
| -------------------- | ----------- | --------- |
|`admin@test.com`      |`0adminTest;`| Admin     |
|`prevoznik1@test.com` |`Prevoznik1!`| Prevoznik |
|`prevoznik2@test.com` |`Prevoznik1!`| Prevoznik |
|`prevoznik3@test.com` |`Prevoznik1!`| Prevoznik |
|`prevoznik34@test.com`|`Prevoznik1!`| Prevoznik |
|`inspektor1@test.com` |`Inspekt0r! `| Inšpektor |
|`inspektor2@test.com` |`Inspekt0r! `| Inšpektor |

Če želite bazo ponastaviti, poženite `docker compose restart server`.

Vse zaustavite in pobrišete podatke z ukazom `docker compose down -v`.