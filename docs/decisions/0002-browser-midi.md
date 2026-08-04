# ADR-0002: Process live MIDI in the browser

- Status: Accepted
- Date: 2026-08-04

## Context

The digital piano is connected by USB to the learner's laptop or tablet, while Nadiano runs in a Docker container that may be hosted on another machine. Sending every MIDI event to the server would add network latency and would not give the container direct access to the client's USB device.

## Decision

Use the Web MIDI API in the learner's browser. Normalize events and run the active practice clock and live matcher locally. Send completed, normalized attempt results to the server through idempotent endpoints.

Require HTTPS externally and explicit user permission. Detect capabilities instead of assuming browser support.

## Consequences

- low-latency interaction does not depend on the local network round trip;
- server deployment remains independent of USB pass-through;
- browser support is limited and must be documented;
- important scoring rules need equivalent shared fixtures across browser and core implementations;
- raw MIDI need not leave the learner device by default.

## Alternatives considered

- USB pass-through to Docker: rejected because the piano is attached to the client device and deployments vary.
- Server-side event streaming: rejected for live scoring because it introduces unnecessary latency and disconnect sensitivity.
- Native desktop wrapper: rejected for 1.0 because the product goal is a simple web application.

## Reconsideration triggers

- a native application becomes an explicit product target;
- Web MIDI support becomes inadequate for required target devices;
- a local companion service is justified by validated user demand and reviewed security implications.
