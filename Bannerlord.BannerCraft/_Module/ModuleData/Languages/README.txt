BannerCraft translations
========================

std_BannerCraft.xml contains every text BannerCraft adds (crafting screen, popups, messages and the MCM settings).
Texts that BannerCraft reuses from the base game are already translated by the game and aren't listed.

To add a language, for example Russian:

1. Create the folder ModuleData/Languages/RU

2. Copy std_BannerCraft.xml to RU/std_BannerCraft_rus.xml and in that copy:
   - change <tag language="English" /> to the game's name for the language, e.g. <tag language="Русский" />
   - translate the text="..." values, keep every id="..." unchanged
   - keep placeholders such as {HERO}, {ITEM}, {REQUIRED} as they are

3. Create RU/language_data.xml:

   <?xml version="1.0" encoding="utf-8"?>
   <LanguageData id="Русский">
     <LanguageFile xml_path="RU/std_BannerCraft_rus.xml" />
   </LanguageData>

The language id must match the one the game uses, see Modules/Native/ModuleData/Languages/<LANG>/language_data.xml.

Ids don't change between updates. When a new version adds texts, they show up in std_BannerCraft.xml with new ids,
so existing translations keep working and only the new lines need translating.
