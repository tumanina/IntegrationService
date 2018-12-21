It's prototype of integration between old and new systems.

Workflow:
1. When in old system ('Admin') staff of other systems changes any information (product quantity, name, price, etc) event about it send to message queue ('event hub' in company terminology).
2. IntegrationService listen that event hub, read the event and asked old system for additional information if it nessesary.
3. IntegrationService  create new event and send it for all subscribers who need to receive it (for example one subscriber want to read events for all countries, but some  other only for 'DA', so that if event.CountryId == 'DA' it's sent for both event hubs, if not - only for one, who need events for all countres).
4. New system (one of subscribers) listen event hub, take the event and ask about additional information by API, for example send GET request like '/api/v1/products/1B64EA41-9E6F-4D67-857A-816415F6C040_7FC9E2A1-BDAE-4690-89A5-99E1F3CF5A26_DA'.
