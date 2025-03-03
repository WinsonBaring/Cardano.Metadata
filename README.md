
for local development
```
This application has a worker and a web api.
```
make sure to add github PAT
```
in the app settings.sjon
```
The worker
```
the GithubWorker.cs do 2 things , 
-it will first do full sync if the database has not been sync with the github 
-it will then do incremental sync every n minutes

Optional: 
- there is a 'savefile function' in the github worker to know the "fetched data structure"

there are 2 models that the worker is being enforced with
- The database model
- The model of the data that is being fetched from github (GitCommit, GitTreeResponse, GitCommitFile)
```
Web Api
```
expose the data stored by the worker
```



## Running the Appication



docker command to run postgres to have db
```
docker run --hostname=e6b50492ec62 --mac-address=02:42:ac:11:00:02 --env=POSTGRES_USER=pg --env=POSTGRES_DB=pg --env=POSTGRES_PASSWORD=pg --env=PATH=/usr/local/sbin:/usr/local/bin:/usr/sbin:/usr/bin:/sbin:/bin:/usr/lib/postgresql/17/bin --env=GOSU_VERSION=1.17 --env=LANG=en_US.utf8 --env=PG_MAJOR=17 --env=PG_VERSION=17.2-1.pgdg120+1 --env=PGDATA=/var/lib/postgresql/data --volume=/var/lib/postgresql/data --network=bridge --workdir=/ -p 5699:5432 --restart=no --runtime=runc -d postgres:latest
```

migrate db
```
dotnet ef migrations add Init (if not yet initialized)
dotnet ef database update (if there is migration file)
```

run project
```
dotnet run --project src/Metadata/Metadata.csproj
```



