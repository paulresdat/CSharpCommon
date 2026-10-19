# Dependency Security

When restore reports a vulnerable package, identify the direct dependency that
introduced it and determine whether a compatible patched version exists.

- Prefer the smallest safe version update.
- Review lockfile changes as part of the dependency update.
- Build and run focused tests after restoring.
- Do not suppress an advisory without documenting the reason and scope.
