# LocalAI

LocalAI is an offline-first desktop AI assistant built with C# and .NET.

The goal of this project is to run open-source Large Language Models (LLMs)
locally on a user's machine without requiring a cloud AI API for inference.

## Project Status

🚧 Under Development

## Goals

- Run open-source LLMs locally
- Support local GGUF models
- Provide a desktop chat interface
- Stream LLM responses
- Store conversations locally
- Support local memory
- Support Retrieval-Augmented Generation (RAG)
- Support local code generation
- Support extensible AI tools
- Support agent-style workflows
- Keep user data local

## Technology

- C#
- .NET
- WPF
- SQLite
- llama.cpp
- GGUF
- GitHub

## Architecture

The application is designed using a modular architecture so that
future capabilities can be added without tightly coupling the core
application to a specific AI runtime.

```text
LocalAI
│
├── Desktop
│
├── Application
│
├── Domain
│
├── Infrastructure
│
└── Inference