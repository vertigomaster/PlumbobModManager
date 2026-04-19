TODO - Just got started on this project.

## Building Notes
- For Windows: `dotnet publish -c Release -r win-x64 --self-contained`
- For Linux: pending but possible
- For Mac: pending but possible

# Vocab
## Library
The collection of all mod entries across all rigs.
## Mod Entry
A specific version or variant of a mod which occupies its own folder in the library.

## Mod Metadata
The metadata associated with a mod entry. Specifies the details that make that entry what it is. 

This data can be stored on entries or on their collective mod. Storing on the mod enables ease of data entry and supplies "defaults" while storing them on entries enables supporting changes across versions/variants.

## Mod
An identifiable game mod. 
Represented by 1 or more mod entries if variants or different versions are available/known to the library.

## Mod Pack
A collection of mods which together provide some identifiable functionality, such that enabling/disabling them together makes sense. 

The main thing that makes these distinct from categories is that metadata can be stored on the pack itself, enabling options like whether to enforce partial enabling/disabling of mods/entries within the pack.  

Examples could include a zombie mod coupled with a survival mod and some aesthetically relevant CC.

## Tag
A marker which can be applied to mods, packs, or their entries.
Tags can be hierarchical, with parent tags allowing for grouping and filtering of child tags.

Some will be automatically provided, but users can create their own.

### Category
A tag under the hood which is used to organize mods by category. 

Under the hood, they're just tags - nothing special; the difference is merely semantic.

### Role
A tag which can be used to describe the impact or role of a entry, mod, pack, or rig.

things like "required" or "recommended" or "nice to have"

## Mod Rig
A collection of mods which are deployed together.

Before launching the game, a rig should be selected, and its mod entries will be deployed to the game's mods folder.
## Mod Profile
The page describing a mod and all its associated details and metadata, like who made it, where it came from, what it does, what rigs it is active in, its category, etc. 

This is a front-end concept, and pulls a lot of data together. 
