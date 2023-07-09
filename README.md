# MusicMove - Simple music file mover / renamer
That's all. Drag-and-Drop music files / folders to move.

NOTE: Formats are hardcoded. You may need to change these.

## Usage

1. File list mode:
`<executable> @"<list file path>"`

2. Drag-and-Drop mode:
`<executable> <file1> [file2] [file3] ...`

## Modes

1. 'Move by artist name' mode (Default)
Execute the program without any special environment variables set.

2. Parse cover mode
Set env var `MM_COVER` to '1' and execute.

Supported album cover image extension: `.jpg`, `.png`, `.webp`

3. Move instrumentals mode
Set env var `MM_INSTRU` to '1' and execute.

4. Rename instrumentals mode
Set env var `MM_RINSTRU` to '1' and execute.

5. Name normalizer mode (for `<artistName> - <songName>` format)
Set env var `MM_NAME_NORMALIZE` to '1' and execute.

6. S3RL-style name normalizer mode (for `<songName> - <artistName>` format)
Set env var `MM_NAME_NORMALIZE_S3RL` to '1' and execute.

7. Tag updater mode (Update ID3 tags according to the information parsed from file name)
Set env var `MM_TAGS` to '1' and execute.

Warning: All ID3v1 tags will be dropped and replaced with ID3v2 tags

8. Import album mapping mode
Set env var `MM_ALBUM_MAPPING` to **your album mapping file path** and execute.

9. Import information dump CSV mode
Set env var `MM_DUMP` to **your information dump CSV file path** and execute.
