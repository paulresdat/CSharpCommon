# CLI Parser

Preserve declarative DTO binding and explicit property types when changing CLI
parsing behavior. New parser implementations should coexist with the legacy
interface until compatibility is proven.

- Test aliases, typed values, missing values, unknown values, help, and custom
  interpreters.
- Keep output injectable through the existing console abstraction.
- Avoid coupling the public API to a specific parser package.
