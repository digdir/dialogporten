// This file is generated, see "scripts" directory
import { default as authentication } from './authentication.js';
import { default as authorization } from './authorization.js';
import { default as dialogCreateActivity } from './dialogCreateActivity.js';
import { default as dialogCreateExternalResource } from './dialogCreateExternalResource.js';
import { default as dialogCreateInvalidActionCount } from './dialogCreateInvalidActionCount.js';
import { default as dialogCreateInvalidProcess } from './dialogCreateInvalidProcess.js';
import { default as dialogCreatePatchDelete } from './dialogCreatePatchDelete.js';
import { default as dialogCreateUpdatePatchDeleteCorrespondenceResource } from './dialogCreateUpdatePatchDeleteCorrespondenceResource.js';
import { default as dialogSearch } from './dialogSearch.js';
import { default as dialogUpdateActivity } from './dialogUpdateActivity.js';
import { default as concurrency } from './concurrency.js';

export default function() {
  authentication();
  authorization();
  dialogCreateActivity();
  dialogCreateExternalResource();
  dialogCreateInvalidActionCount();
  dialogCreateInvalidProcess();
  dialogCreatePatchDelete();
  dialogCreateUpdatePatchDeleteCorrespondenceResource();
  dialogSearch();
  dialogUpdateActivity();
  concurrency();
}
