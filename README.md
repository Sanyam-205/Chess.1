# Unity Chess – Full Rule Implementation

## Overview
A fully functional classical chess game built in Unity using C#.  
This project focuses on accurate rule enforcement, clean move validation, and proper handling of all standard chess mechanics.

The goal of this version (V1) was to implement a complete and correct chess rules engine before moving toward AI development in a future version.

---

## Features Implemented

- Legal move generation for all pieces
- Path blocking for sliding pieces (rook, bishop, queen)
- Check detection
- Checkmate detection
- Stalemate detection
- Insufficient material draw detection
- Castling (kingside and queenside)
- En passant
- Pawn promotion
- Visual highlighting of legal moves
- King highlighted when in check
- Scene reload functionality

This version fully supports standard classical chess rules.

---

## Architecture Overview

- Board represented as a 2D array
- Move simulation used to validate king safety
- Separation between move validation logic and visual rendering
- State tracking for:
  - Castling rights
  - En passant conditions
  - Piece movement flags

Legal moves are filtered from pseudo-legal moves by simulating board states and verifying king safety.

---

## Controls

- Click a piece to view legal moves
- Click a highlighted square to move
- Pawn promotion uses hotkeys:
  - Q → Queen
  - R → Rook
  - B → Bishop
  - N → Knight
- Reload scene option available

---

## Build

- Built in Unity
- WebGL version available on itch.io

---

## Future Improvements (V2)

- AI opponent (Minimax + Alpha-Beta pruning)
- Cleaner separation between logic layer and UI layer
- Improved promotion UI
- Move history & undo system
- Performance optimizations

---

## Tech Stack

- Unity
- C#
- WebGL

---

## Learning Outcomes

This project strengthened understanding of:

- State management
- Move simulation and validation
- Rule-based game systems
- Edge case handling (castling, en passant)
- Game architecture design in Unity

---

## Play Online

WebGL build available on itch.io:  
[Play Here](https://sanyam-20.itch.io/chess1)

---

## Status

Version 1 complete – full classical chess rules implemented.
