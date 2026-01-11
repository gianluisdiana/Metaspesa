import { defineConfig, globalIgnores } from 'eslint/config';
import nextVitals from 'eslint-config-next/core-web-vitals';
import nextTs from 'eslint-config-next/typescript';
import tseslint from 'typescript-eslint';

const eslintConfig = defineConfig([
  tseslint.configs.recommended,
  ...nextVitals,
  ...nextTs,
  {
    languageOptions: {
      ecmaVersion: 'latest',
    },
    rules: {
      '@typescript-eslint/no-magic-numbers': [
        'error',
        {
          ignore: [0, 1, 2],
          ignoreEnums: true,
          ignoreReadonlyClassProperties: true,
        },
      ],
      curly: ['error', 'multi-line', 'consistent'],
      'dot-notation': 'error',
      eqeqeq: 'error',
      'max-nested-callbacks': ['error', { max: 4 }],
      'no-empty-function': 'error',
      'no-eval': 'error',
      'no-inline-comments': 'error',
      'no-lonely-if': 'error',
      'no-magic-numbers': 'off',
      'no-multi-assign': 'error',
      'no-negated-condition': 'error',
      'no-nested-ternary': 'error',
      'no-new': 'error',
      'no-new-wrappers': 'error',
      'no-param-reassign': 'error',
      'no-sequences': 'error',
      'no-shadow': ['error', { allow: ['err', 'resolve', 'reject'] }],
      'no-throw-literal': 'error',
      'no-useless-catch': 'error',
      'no-useless-computed-key': 'error',
      'no-useless-concat': 'error',
      'no-useless-constructor': 'error',
      'no-useless-rename': 'error',
      'no-useless-return': 'error',
      'no-var': 'error',
      'no-with': 'error',
      'object-shorthand': ['error', 'always', { avoidQuotes: true }],
      'operator-assignment': ['error', 'always'],
      'prefer-const': 'error',
      'prefer-destructuring': [
        'error',
        {
          array: true,
          object: true,
        },
      ],
      'prefer-exponentiation-operator': 'error',
      'prefer-named-capture-group': 'error',
      'prefer-object-spread': 'error',
      'prefer-promise-reject-errors': 'error',
      'prefer-regex-literals': ['error', { disallowRedundantWrapping: true }],
      'prefer-rest-params': 'error',
      'prefer-template': 'error',
      'require-await': 'error',
      'sort-keys': 'error',
      yoda: 'error',
    },
  },
  globalIgnores([
    // Default ignores of eslint-config-next:
    '.next/**',
    'out/**',
    'build/**',
    'next-env.d.ts',
  ]),
]);

export default eslintConfig;
