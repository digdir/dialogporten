dev: ## Start up all containers
	podman compose up -d

no-webapi: ## Start up containers without WebAPI/GraphQL
	podman compose -f no-webapi.compose.yml up -d

down: ## Shuts down all containers
	podman compose down

# ---------------------
# - Helper functions  -
# ---------------------

.PHONY: help select no-webapi dev down
.DEFAULT_GOAL := select

select:
	@make help | sed '1,2d' | \
		fzf --ansi --bind "enter:execute(make {1} < /dev/tty > /dev/tty 2>&1)+abort" || true

# Help command taken from: https://marmelab.com/blog/2016/02/29/auto-documented-makefile.html
help:
	@echo "Usage: make [task]\n"
	@grep -E '^[a-zA-Z_-]+:.*?## .*$$' $(MAKEFILE_LIST) | sort | awk 'BEGIN {FS = ":.*?## "}; {printf "\033[36m%-30s\x1b[0m %s\n", $$1, $$2}'
