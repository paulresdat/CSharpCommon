# API Compatibility

Before changing a public type, interface, constructor, or method signature,
inspect its usages and preserve the existing contract when possible.

- Prefer additive APIs for migrations.
- Keep legacy implementations working while replacements are evaluated.
- Add tests for both the legacy and replacement paths.
- Treat public signature changes as deliberate compatibility decisions.
