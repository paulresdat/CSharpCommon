# Integration Testing

Use the repository's integration-test base classes and fixtures. Tests should
exercise real provider behavior while keeping transaction and cleanup behavior
explicit when the provider cannot support ambient transactions.

- Inherit from the established base test structure.
- Prefer fixture-managed setup and cleanup.
- Add a focused regression test for each persistence behavior change.
- Record provider limitations rather than hiding them in test helpers.
