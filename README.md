# LocalAI

LocalAI is an offline-first desktop AI assistant built with C# and .NET.

It runs compatible open-source GGUF language models locally on your Windows machine, allowing you to chat with an AI without sending your conversations to a cloud AI service.

## Features

- 🖥️ Modern Windows desktop UI
- 🔒 Offline-first local AI
- 🤖 Local GGUF model support
- ⚡ Streaming AI responses
- 🛑 Stop generation
- 💬 Multiple conversations
- 💾 Local conversation persistence
- 🔄 Model selection and switching
- 🗃️ SQLite-based local storage
- 📦 Self-contained Windows publishing
- 🌐 No cloud API required for inference

## Current Status

The current version supports:

- Local GGUF model discovery
- GGUF model loading
- Streaming chat
- Conversation history
- Conversation persistence
- Automatic conversation titles
- Multiple model selection
- Model switching
- Offline execution
- Fresh-install database initialization

## Requirements

### Runtime

Windows x64.

The published application is self-contained, so users do not need to install the .NET runtime separately.

### Hardware

Local model performance depends heavily on your hardware and the selected model.

CPU inference is supported.

GPU acceleration support will be expanded in future versions.

## Getting Started

### 1. Download LocalAI

Download the latest Windows release from the GitHub Releases page.

### 2. Download a compatible GGUF model

LocalAI does not bundle large language models with the application.

Download a compatible GGUF instruction/chat model from a trusted model provider such as Hugging Face.

Make sure the model is compatible with the supported inference runtime.

### 3. Place the model in the LocalAI models directory

Create:

```text
%LOCALAPPDATA%\LocalAI\models

## Code Signing Policy

LocalAI is an open-source Windows desktop application distributed through
GitHub Releases.

**Free code signing provided by SignPath.io, certificate by SignPath Foundation.**

### Code Signing Roles

LocalAI is currently maintained by the project owner.

- **Committers and reviewers:** LocalAI project maintainer
- **Approvers:** LocalAI project maintainer

All release binaries submitted for signing are built from the LocalAI
source repository using the project's automated GitHub Actions build process.

### Privacy Policy

LocalAI is designed as an offline-first application.

For the current privacy policy, see
[PRIVACY.md](PRIVACY.md).

LocalAI does not transfer user conversations, prompts, AI-generated responses,
or locally stored conversation data to external networked systems for
inference unless specifically requested by the user or operator.