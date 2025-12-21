# NABS Migrations

I started a new project recently that involved ongoing experimentation and changes to my SQL database schema. I normally just use `dotnet ef migrations` commands to manage database schema changes. In this case, I found myself frequently needing to create, apply, roll back, and sometimes resetting all migrations and even the whole database. Eventually, I had some PowerShell scripts to help with this. There were a couple of issues:

- Maintaining the scripts was cumbersome.
- Typing commands in the terminal was error-prone and tedious.
- I kept on forgetting the exact sequence of commands needed for certain operations, expecially after a couple of days between iterations.

I wanted a more streamlined way to handle migrations and also a way to provide guidance and discoverability of the various commands. That's when I decided to create a wrapper CLI tool with the following goals:

- Provide a statistical summary of migrations in a solution. Number of tables, number of migrations per project/DbContext.
- Visualise all pending model changes in a solution so that I could easily see what changes did not have a migration. It was also important to see which midel change would result in a destructive migration.
- Visualise all migrations in a solution so that I could see the order of migrations and which ones had been applied to the database or not.
- Simplify the process of managing SQL migrations.
- Provide developers with the ability to understand and control migrations easily.
- Offer a user-friendly command-line interface for common migration tasks.
- Support multiple projects within a solution.
- Support multiple DbContexts within a project.
- Enable easy rollback of migrations.
- Provide easy capability to drop local databases and migrate them from scratch.
- Facilitate automated migration deployment in CI/CD pipelines.