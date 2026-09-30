# Privacy Policy

**Last updated:** September 30, 2026

LocalAI is an offline-first desktop application designed to run compatible
open-source AI models locally on the user's Windows computer.

## Data Processing

LocalAI is designed to process the following data locally on the user's
computer:

- User prompts and messages
- AI-generated responses
- Conversation history
- Locally configured AI models
- Local application settings

Conversation history is stored locally using SQLite under the user's local
application data directory.

LocalAI does not send user conversations, prompts, AI-generated responses,
or locally stored conversation data to a cloud AI service for inference.

## Network Access

LocalAI does not require a cloud AI service for inference.

Users may independently download the LocalAI application or compatible AI
models from external websites such as GitHub or model providers. Such
downloads are initiated by the user and are outside the LocalAI application's
local inference process.

## Third-Party Services

LocalAI does not currently require an external account or cloud AI API to
perform local inference.

Third-party software libraries used by LocalAI may have their own privacy
policies and terms. Users should review the policies of any external service
they choose to use for downloading models or other resources.

## Local Storage

LocalAI stores application data locally on the user's computer.

The current application stores conversation data in:

`%LOCALAPPDATA%\LocalAI\data\LocalAI.db`

AI models are stored locally in:

`%LOCALAPPDATA%\LocalAI\models`

## Changes to This Policy

This privacy policy may be updated when LocalAI introduces functionality
that changes how user data is processed.

Any significant changes will be documented in the project repository.

## Contact

For questions about this privacy policy or LocalAI, please use the project's
GitHub repository:

https://github.com/Piklugithub/LocalAI