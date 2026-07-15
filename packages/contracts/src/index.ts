/**
 * @packages/contracts
 *
 * Minimal contracts package containing type definitions for the Watcher Database Infrastructure.
 * This package provides shared interfaces used by the database layer and other Watcher components.
 *
 * These are type-only exports - no implementation logic is included here.
 */

// State management types
export type { FileStatus } from './state/file-status';
export type { WatcherState } from './state/watcher-state';
export type { StateRepository } from './state/state-repository';

// Configuration types
export type { InterfaceConfig } from './config/interface-config';
export type { ConnectionConfig } from './config/connection-config';
