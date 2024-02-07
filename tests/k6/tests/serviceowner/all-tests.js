// This file is generated, see "scripts" directory
import { default as authentication } from './authentication.js';
import { default as authorization } from './authorization.js';
import { default as concurrency } from './concurrency.js';
import { default as dialogCreateInvalidActionCount } from './dialogCreateInvalidActionCount.js';
import { default as dialogCreatePatchDelete } from './dialogCreatePatchDelete.js';
import { default as dialogSearch } from './dialogSearch.js';

export default function() {
  authentication();
  authorization();
  concurrency();
  dialogCreateInvalidActionCount();
  dialogCreatePatchDelete();
  dialogSearch();
}
