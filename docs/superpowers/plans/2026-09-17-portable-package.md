# XL-OCV-256CH Portable Package Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Build a reproducible `XL-OCV-256CH-Portable.zip` containing the Release|x86 application runtime and an offline Chinese HTML user manual.

**Architecture:** Keep the application unchanged. Store release documentation sources under `packaging/`, and use `tools/Build-PortablePackage.ps1` to build the existing VB.NET solution, copy only runtime files into a staging directory, validate required artifacts, and create the ZIP under `dist/`.

**Tech Stack:** VB.NET Windows Forms, .NET Framework 4.8, MSBuild, PowerShell, System.IO.Compression, standalone HTML/CSS.

---

## File Map

- Create: `packaging/使用说明书.html` — single-file, offline Chinese operating manual with embedded CSS.
- Create: `packaging/运行说明.txt` — short extraction and prerequisite checklist placed beside the executable.
- Create: `tools/Build-PortablePackage.ps1` — reproducible Release|x86 build, staging, validation, and ZIP creation script.
- Generate: `dist/XL-OCV-256CH-Portable.zip` — final delivery artifact; do not commit generated output unless explicitly requested.
- Generate: `dist/staging/XL-OCV-256CH/` — script-owned temporary directory; recreate it on each run.
- Do not modify: application `.vb`/`.vbproj` files, the existing user changes, or existing build-verification directories.

### Task 1: Create the offline user documentation

**Files:**
- Create: `packaging/使用说明书.html`
- Create: `packaging/运行说明.txt`

- [ ] **Step 1: Write the HTML document skeleton and navigation.**

Create a UTF-8 HTML5 document with the title `XL-OCV-256CH 系统软件使用说明书`, an embedded stylesheet, a visible table of contents, and sections for environment, startup, login/permissions, initial settings, device connections, testing, calibration, data/logs, troubleshooting, and backup.

- [ ] **Step 2: Fill the procedure sections from the current application terminology.**

Use the existing menu/form names (`初始设置`, `调试`, `用户管理`, `日志查询`, `通道校准`, `手动测试`, `正式测试`) and describe actions in short numbered procedures. Mark any field values that depend on the actual station as site-specific instead of inventing COM ports, database addresses, or instrument addresses.

- [ ] **Step 3: Document external prerequisites and limitations.**

Include explicit callouts stating that the target computer needs .NET Framework 4.8, VISA software for instrument communication, compatible Microsoft Office for Excel export, and the applicable PLC/serial/device drivers and network services. Explain that the portable package does not install or register those external components.

- [ ] **Step 4: Add the extraction/run checklist text file.**

Write `运行说明.txt` with these exact operational points: extract the entire folder, use a writable path such as `C:\XL-OCV-256CH`, run `XL_OCV_256CH.exe`, do not delete DLL/config/data files, open `使用说明书.html`, and verify VISA/Office/device prerequisites when related functions are needed.

- [ ] **Step 5: Check documentation structure and encoding.**

Run:

```powershell
rg -n "XL-OCV-256CH|系统要求|初始设置|通道校准|常见故障|备份|VISA|Office" packaging/使用说明书.html packaging/运行说明.txt
```

Expected: each required topic is found, and no unfinished placeholder text is present.

- [ ] **Step 6: Commit the documentation.**

```powershell
git add -- packaging/使用说明书.html packaging/运行说明.txt
git commit -m "docs: add portable package user manual"
```

### Task 2: Implement the reproducible packaging script

**Files:**
- Create: `tools/Build-PortablePackage.ps1`

- [ ] **Step 1: Define safe script parameters and paths.**

The script must accept `-Configuration` defaulting to `Release`, `-Platform` defaulting to `x86`, and `-OutputRoot` defaulting to `<repository>\dist`. Resolve the repository from the script location, and use only the script-owned paths `<OutputRoot>\staging` and `<OutputRoot>\XL-OCV-256CH-Portable.zip` for cleanup/overwrite.

- [ ] **Step 2: Locate MSBuild and build the existing solution.**

Search in this order: `Get-Command msbuild.exe`, Visual Studio Installer `vswhere.exe`, and the standard Visual Studio MSBuild paths. Fail with a clear prerequisite message if none exists. Invoke `MP-60A-RV-64Mem.sln` with `/t:Rebuild /p:Configuration=Release /p:Platform=x86`, stop on a nonzero exit code, and use the produced `bin\Debug` output because this project’s Release|x86 configuration currently maps to that directory.

- [ ] **Step 3: Recreate staging and copy runtime files.**

Create `<OutputRoot>\staging\XL-OCV-256CH`, copy the built application output while excluding `.pdb`, `.xml`, `.application`, `app.publish`, and other build metadata, then copy `packaging/使用说明书.html` and `packaging/运行说明.txt`. Preserve the executable config file, manifest when present, project/runtime DLLs, `Initset.set`/`bar.set` templates when present, and other data files required by the output.

- [ ] **Step 4: Validate required files before compression.**

Stop with a nonzero exit code unless staging contains `XL_OCV_256CH.exe`, `XL_OCV_256CH.exe.config`, `XL_OCV_UIControls.dll`, `使用说明书.html`, and `运行说明.txt`. Also fail if staging contains `.pdb`, `.xml`, `.application`, or an `app.publish` directory.

- [ ] **Step 5: Create the ZIP with a stable top-level folder.**

Remove only the script-owned prior ZIP and staging directory, then call `Compress-Archive` on the staging top-level directory so extraction creates `XL-OCV-256CH\`. Print the absolute ZIP path, file count, and byte size.

- [ ] **Step 6: Commit the script.**

```powershell
git add -- tools/Build-PortablePackage.ps1
git commit -m "build: add reproducible portable package script"
```

### Task 3: Build and verify the delivery artifact

**Files:**
- Generate: `dist/XL-OCV-256CH-Portable.zip`
- Generate: `dist/staging/XL-OCV-256CH/`

- [ ] **Step 1: Execute the package script from the repository root.**

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\tools\Build-PortablePackage.ps1
```

Expected: exit code `0`, a Release|x86 build, required-file validation success, and an absolute ZIP path.

- [ ] **Step 2: Verify the ZIP entries independently.**

Use `[System.IO.Compression.ZipFile]::OpenRead()` to assert that the archive contains `XL-OCV-256CH/XL_OCV_256CH.exe`, `XL-OCV-256CH/XL_OCV_256CH.exe.config`, `XL-OCV-256CH/使用说明书.html`, and `XL-OCV-256CH/运行说明.txt`, and contains no `.pdb`, `.xml`, `.application`, or `app.publish` entries.

- [ ] **Step 3: Verify extraction into an isolated temporary directory.**

Extract the ZIP into a newly created temporary directory outside the repository, assert that the same required files exist, and compare the extracted file count with the archive’s non-directory entry count.

- [ ] **Step 4: Perform a process-start smoke test.**

Start the extracted `XL_OCV_256CH.exe` with its working directory set to the extracted folder, wait up to 10 seconds for a process, record whether startup succeeds, and terminate only that smoke-test process. Report device/VISA/Office/database checks separately as requiring the target station environment.

- [ ] **Step 5: Inspect the final worktree and delivery path.**

Run:

```powershell
git status --short
Get-Item .\dist\XL-OCV-256CH-Portable.zip | Select-Object FullName,Length,LastWriteTime
```

Confirm that only the intended source files are tracked changes and that the ZIP exists. Preserve unrelated pre-existing changes and temporary directories.
