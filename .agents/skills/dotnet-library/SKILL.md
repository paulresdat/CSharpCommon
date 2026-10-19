# .NET Library

Treat this repository as a reusable library, not as an application. Prefer
small public APIs, preserve existing behavior, and avoid introducing runtime
assumptions that consuming applications do not require.

Validation:

- Restore with the repository's configured package-lock policy.
- Build the narrowest affected project first.
- Run focused tests before broader solution tests.
- Report warnings separately from failures.
