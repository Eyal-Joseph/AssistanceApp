
# AssistanceApp Setup Instructions


## 1. Add `appsettings.Development.json` File

- Create an `appsettings.Development.json` file in each project that requires it (e.g., `Assistance/`, `AssistanceAPI/`, `AssistanceHandler/`).
- Copy the structure from `appsettings.json` or request the required keys from the project owner.
- This file should contain your local development configuration (connection strings, API keys, etc.).
- **Do not commit sensitive information to version control.**

### Required Properties Example

Add the following properties to your `appsettings.Development.json` as needed:

```json
{
	"GeminiModelName": "<your-gemini-model-name>",
	"GeminiEmbeddingModel": "<your-gemini-embedding-model>",
	"GeminiApikey": "<your-gemini-api-key>",
	"AzureModelName": "<your-azure-model-name>",
	"AzureEmbeddingModel": "<your-azure-embedding-model>",
	"AzureEndpoint": "<your-azure-endpoint>",
	"AzureApikey": "<your-azure-api-key>"
}
```

Replace the placeholder values with your actual configuration.

## 2. React UI Project: Install Node Modules

- Navigate to the React UI directory:
	```powershell
	cd AssistanceInterface/ui
	```
- Install all dependencies (run these commands in order):
	```powershell
	npm install
	npm audit fix --force
	npm update
	```
- To start the development server:
	```powershell
	npm start
	```

## 3. Docker Installation

- Make sure Docker Desktop is installed on your machine: [Download Docker Desktop](https://www.docker.com/products/docker-desktop/)
- Start Docker Desktop before running any Docker commands.
- To build and run the containers:
	```powershell
	docker-compose up --build
	```
- To stop the containers:
	```powershell
	docker-compose down
	```

For more details, refer to the documentation or contact the project maintainer.

---

## Credits

This project was created and developed by Eyal Joseph.

