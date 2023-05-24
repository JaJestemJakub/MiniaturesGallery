# MiniaturesGallery
This is example aplication of instargam like aplication. You are able to add post, then attach photos, rate posts and add comment to them. It was created with intention to collect wargaming models photos, but in fact it doesn't have any specific feature for this purpose.

To operate application need database. After establishing connection string in appsetings.json, you can use PMC with update-database to create tables using Entity Framework. 
It is needed to set admin password with "dotnet user-secrets set SeedAdminUserPW <pw>". Admin name will be "admin@admin.com".
