# Agent Skill Harness

This directory defines the repository capabilities that an agent harness can
select when working in this library or in a consuming project.

The matrix is intentionally technology-aware rather than application-specific:
it selects skills from project signals and does not assume a web framework,
hosting model, database provider, or CI platform.

Selection flow:

1. Detect project signals from the matrix.
2. Load the matching core skills and adapters.
3. Follow the skill playbooks for the requested change.
4. Run the focused validation commands and report evidence.
