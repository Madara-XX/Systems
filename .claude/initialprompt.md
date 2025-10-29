Rola: Jesteś moim współprogramistą Unity C# pracującym w trybie HumanLayer. Nie przekraczaj 60% kontekstu. Pracujemy tylko nad małym wycinkiem: system Inventory.
Silnik/wersje: Unity 2022/2023 LTS.
Zasady architektury:

Kontrakty (interfejsy/DTO) w Assets/Contracts/.

Implementacja usług w Assets/Systems/Inventory/.

MonoBehaviours cienkie; logika domenowa w czystych klasach C#.

Konfiguracja przez ScriptableObjects (później), zasoby przez Addressables (poza zakresem tego etapu).

Brak kontenera DI; dla usług preferuj jawne referencje / proste fabryki.

Testy: Unity Test Framework (EditMode) w Assets/Tests/EditMode/InventoryTests.cs.

API publiczne minimalne, nazewnictwo jednoznaczne.

Każdy etap kończ krótkim podsumowaniem do thoughts/.

Cel tego promptu: Przygotuj starter HumanLayer i szkielety pod system Inventory bez dotykania scen/prefabów.

1) Wygeneruj zawartość plików HumanLayer (Markdown do wklejenia)

Przygotuj treść dla czterech plików do folderu:
thoughts/INVENTORY-YYYY-MM-DD/ (użyj dzisiejszej daty w nazwie katalogu)

research.md

plan.md

implement.md

validate.md

Wymagania treści (zwięźle, rzeczowo):

research.md: problem, zakres, zależności (Character, Save, UI, DropTable – poza zakresem teraz), ryzyka, definicje pojęć, “out-of-scope”.

plan.md: kontrakty do stworzenia, klasy implementacyjne, testy (3–5 przypadków), kolejność kroków, kryteria akceptacji.

implement.md: lista plików do utworzenia/edytowania (BEZ scen), minimalny publiczny API, reguły wyjątków, zdarzenia i ich ładunki, TODO na później (UI/Save).

validate.md: jak uruchomić testy EditMode, oczekiwane wyniki, checklista debugowania, miejsce na log z testów (pusty blok do wypełnienia przeze mnie).

Dodatkowo do research.md dodaj krótką glosariusz nazw (Item, ItemStack, Slot, CapacityPolicy, FilterPolicy).

sam odpal npx humanlayer thoughts init inicjując humanlayer. 