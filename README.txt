# Graphium - Real-Time Signal Visualization

## IMPORTANT: First-Time Setup

Before running Graphium, open Command Prompt as Administrator and run:

```
netsh http add urlacl url=http://+:8080/ user=Everyone
```

(Replace 8080 with your port if different)

This allows the application to accept network connections without admin privileges.

## Verify Setup

```
netsh http show urlacl
```

## Remove Later (if needed)

```
netsh http delete urlacl url=http://+:8080/
```

## Controls

- **F** - Toggle auto-follow
- **Double-click** - Re-enable auto-follow
- **Drag dividers** - Resize plots

## Troubleshooting

**"Access is denied" error?**
Run the netsh command above as Administrator.