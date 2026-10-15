# .NET Version Upgrade

## Preferences
- **Flow Mode**: Automatic; pause if a high-impact decision cannot be inferred safely.
- **Target Framework for Assessment**: `net10.0` (.NET 10 LTS).
- **Assessment confirmation scope**: The target was initially authorized for compatibility assessment only; the user subsequently explicitly authorized implementing the existing LumiStore modernization plan. Implement its selected targets and architecture incrementally, preserving the existing applications until replacements are validated.
- **Commit Strategy**: Not applicable; the workspace has no Git repository.

## Strategy
**Selected**: Bottom-Up (Dependency-First)
**Rationale**: Five projects target .NET Framework 4.7.2 and form a dependency graph from Entidad through Datos/Negocio to the two MVC web applications; assessment reports high-impact compatibility work.

### Execution Constraints
- Complete and validate dependency leaves before dependent libraries and web applications; retain the existing .NET Framework applications while side-by-side replacements are validated.
- Do not change a framework target or architecture until the assessment-led plan and project-specific research support it; preserve the functioning existing deployment path during migration.
- Validate each task with a build, relevant automated and manual QA, and regression checks; address warnings in touched projects.
- Coordinate frontend, backend, and QA work as explicit responsibilities in task research and validation; keep user-facing application content and added documentation in Spanish.
- Do not select or configure a payment provider, use production payment credentials, or activate live transactions without a separate explicit decision; do not deploy without explicit authorization.
- Review the deferred Central Package Management recommendation only after all projects become SDK-style on a single target framework.

## User Preferences
### Technical Preferences
- **Integración con base de datos**: el usuario quiere que LumiStore quede conectada funcionalmente de extremo a extremo con SQL Server `DBCARRITO`, no solo auditada en modo de lectura. Las escrituras CRUD se probarán únicamente en una base aislada autorizada; los cambios DDL en `DBCARRITO` existente requieren confirmación explícita.
- Inventory the repository read-only before source changes.
- Do not perform mass migrations or change framework/architecture without compatibility evidence; ask only when the destination or approach cannot be inferred safely.
- Preserve behavior and run available tests/build validation.
- Do not store credentials/tokens, create or deploy Azure resources, or connect Azure subscriptions without explicit authorization.
- Report verified facts, applied changes, failed tests/builds, and user actions separately.
- **Language**: Keep all user-facing text, application interface text, and new project documentation in Spanish.
- **Delivery quality**: Plan coordinated Frontend, Backend, and QA responsibilities; target responsive, end-to-end functionality and deployment readiness, but do not deploy without explicit authorization.
- **Payment gateway**: Do not select or configure a payment provider, handle real payment credentials, or activate live payments without explicit user direction.
- **Audit scope**: Perform a rigorous read-only review of compatibility findings, automated tests, security/configuration, CI/CD definitions, Azure references, and deployment target/process; distinguish verified evidence from missing repository evidence. Do not authenticate to Azure or modify/deploy resources without explicit authorization.
- **SMTP**: El usuario confirma que la configuración existente es correcta; no indicar que debe revocarse o rotarse ni tratarla como incidente sin evidencia nueva.

### Custom Instructions
#### 04-entidad-moderna
- No repetir los tres intentos fallidos ni usar métodos equivalentes para editar `Entidad.csproj` con la solución abierta. Usar herramientas especializadas cuando existan; si el cambio de TFM requiere editar el proyecto y no hay vía compatible disponible, detenerse y solicitar al usuario que cierre/reanude la solución antes de reanudar. No llamar a `unload_project` sin autorización explícita.

## Key Decisions Log

- User confirmed .NET 10 LTS as the target for compatibility assessment after the solution inventory; the user later explicitly instructed implementation of the existing LumiStore modernization plan, including comprehensive functional/CRUD testing. This authorizes planned code/framework work incrementally and with validation, but not Azure deployment or destructive testing against the current database.
- User requested a formal .NET upgrade plan and full-stack remediation (views, code, payment flow, Spanish, responsive design, QA, deployment readiness); plan before implementation, and preserve the restriction against deploying without explicit authorization.
- No Git repository was found in the workspace or its inspected parent directories; no branch or commit strategy applies.
- Database connection target supplied by user: `(localdb)\MSSQLLocalDB`, Windows Authentication, database `DBCARRITO`; do not persist credentials. Database verification must start read-only; CRUD writes require an isolated test database.
- El usuario aclara que todo lo referente a SMTP es correcto; se descarta la recomendación anterior de revocar/rotar la credencial salvo evidencia nueva.
- User explicitly authorized DDL directly in existing `DBCARRITO` to create the database objects/relationships demonstrated as necessary. This does not authorize CRUD/DML writes or tests that alter business rows. No isolated test database is available; do not create one without further authorization.
- The user has not yet closed/reopened `LumiStore.slnx`; `04-entidad-moderna` remains blocked and dependent modernization tasks must not be skipped.

## Upgrade Options

### Strategy
- Upgrade Strategy: Bottom-Up

### Project Structure
- Project Approach: Web projects=Side-by-side; class libraries=Multi-targeting
- Package Management: Per-Project (defer CPM to post-migration)

### Compatibility
- Unsupported Packages: Resolve Inline (5 incompatible packages)
- Unsupported API Handling: Fix Inline
- System.Web Adapters: Use System.Web Adapters
  Skill: migrating-mvc-system-web-adapters
- Cross-App Cookie Authentication: Remote Authentication

### Modernization
- Assembly Binding Redirects: Document and Review Before Removing
- Nullable Reference Types: Leave Disabled
