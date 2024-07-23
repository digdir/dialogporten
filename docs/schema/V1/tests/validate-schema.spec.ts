import gql from 'graphql-tag';
import { expect, test } from 'vitest';
import { schema_verified_graphql } from '.';

const isValid = (query) => {
  try {
    gql(query);
    return true;
  } catch (err) {
    return false;
  }
};

test('validate', () => {
  const isSchemaValid = isValid(schema_verified_graphql);

  expect(isSchemaValid).toBe(true);
});
