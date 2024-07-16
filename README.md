# MiniaturesGallery
This is example aplication of instargam like aplication. You are able to add post, then attach photos, rate posts and add comment to them. It was created with intention to collect photos of wargaming models, but in fact it doesn't have any specific feature for this purpose.
You can test it by http://jacobfigurines.ddns.net/

Application have SQLite database so it is self contained. If you want to use fresh database file, after establishing connection string in appsetings.json, you can use PMC with update-database to create tables using Entity Framework. 
It is needed to set admin password with "dotnet user-secrets set SeedAdminUserPW <pw>". Admin name will be "admin@admin.com".
