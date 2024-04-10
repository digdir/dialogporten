# Changelog

## [1.5.0](https://github.com/digdir/dialogporten/compare/v1.4.0...v1.5.0) (2024-04-10)


### Features

* **azure:** add azure service bus ([#601](https://github.com/digdir/dialogporten/issues/601)) ([4b008e1](https://github.com/digdir/dialogporten/commit/4b008e176dc9c9f4355b7b101c8a192e357e63fb))

## [1.4.0](https://github.com/digdir/dialogporten/compare/v1.3.0...v1.4.0) (2024-04-09)


### Features

* Split SeenLog from activities ([#598](https://github.com/digdir/dialogporten/issues/598)) ([71b77d2](https://github.com/digdir/dialogporten/commit/71b77d25ca464e4194712e45a19d16d30d17e4d5))
  * *This is a breaking change*, the `Seen` activity type has been removed, and all activities of this type is removed from the staging environment.
* Add EU endpoints for seen log ([#607](https://github.com/digdir/dialogporten/issues/607)) ([1aa7eeb](https://github.com/digdir/dialogporten/commit/1aa7eeb927d21fdea053640edcae7782e278c7cb))
  * `/api/v1/enduser/dialogs/{dialogId}/seenlog`
  * `/api/v1/enduser/dialogs/{dialogId}/seenlog/{seenLogId}`
  * `/api/v1/serviceowner/dialogs/{dialogId}/seenlog`
  * `/api/v1/serviceowner/dialogs/{dialogId}/seenlog/{seenLogId}`
* Add ExtendedStatus content type ([#589](https://github.com/digdir/dialogporten/issues/589)) ([a9f10b0](https://github.com/digdir/dialogporten/commit/a9f10b09ee58453766e1de46acd2166be48fd0b5))
* add fusion cache ([#579](https://github.com/digdir/dialogporten/issues/579)) ([973fa5c](https://github.com/digdir/dialogporten/commit/973fa5c9a68709f8ede71673fa96f06388cf2ea9))
* **azure:** copy from keyvault to app config ([#593](https://github.com/digdir/dialogporten/issues/593)) ([d216c90](https://github.com/digdir/dialogporten/commit/d216c9087472e4e93df913a56b58e79d71becab8))
* **service:** use in-memory transport instead of rabbitmq ([#602](https://github.com/digdir/dialogporten/issues/602)) ([dc339e7](https://github.com/digdir/dialogporten/commit/dc339e77e16c4e1014145a6811474ba15fef2b20))


### Bug Fixes

* Add PartyIdentifier.Separator to party validation error ([#595](https://github.com/digdir/dialogporten/issues/595)) ([14ee4a1](https://github.com/digdir/dialogporten/commit/14ee4a1a1cb57014d7d6d6ae385681fa5b3c690c))
* **azure:** ensure key vault url is correct and add keyvault readerrole for migration job ([#597](https://github.com/digdir/dialogporten/issues/597)) ([2f11a16](https://github.com/digdir/dialogporten/commit/2f11a164b6198848d965ae3a390ba0156204a6e7))

## [1.3.0](https://github.com/digdir/dialogporten/compare/v1.2.0...v1.3.0) (2024-04-03)


### Features

* Add db index for Dialog.Org ([#584](https://github.com/digdir/dialogporten/issues/584)) ([a4c1953](https://github.com/digdir/dialogporten/commit/a4c19530f238e61c046fa9cb8f3b366a7fefebad))


### Bug Fixes

* Require read action on elements without auth attr, replace unauthorized URLs ([#574](https://github.com/digdir/dialogporten/issues/574)) ([f39af31](https://github.com/digdir/dialogporten/commit/f39af3112391a6d6d6df42e6e5cc6d8115c07fd3))

## [1.2.0](https://github.com/digdir/dialogporten/compare/v1.1.1...v1.2.0) (2024-03-22)


### Features

* token issuer ([#556](https://github.com/digdir/dialogporten/issues/556)) ([d8165c1](https://github.com/digdir/dialogporten/commit/d8165c180f4190b3fa243181cd16aa3b007d35a4))

## [1.1.1](https://github.com/digdir/dialogporten/compare/v1.1.0...v1.1.1) (2024-03-22)


### Bug Fixes

* **azure:** avoid naming issue for secrets ([#572](https://github.com/digdir/dialogporten/issues/572)) ([50af860](https://github.com/digdir/dialogporten/commit/50af860d0037eae2611b59393974142f7e55f457))
* UpdateDialogEvent created when dialog element is deleted or updated ([#552](https://github.com/digdir/dialogporten/issues/552)) ([8d707ff](https://github.com/digdir/dialogporten/commit/8d707ffbe8a7a5ff70c053927d8c32cbf5f74410))

## [1.1.0](https://github.com/digdir/dialogporten/compare/v1.0.4...v1.1.0) (2024-03-13)


### Features

* Add name lookups ([#532](https://github.com/digdir/dialogporten/issues/532)) ([db9cadc](https://github.com/digdir/dialogporten/commit/db9cadca7b00bb98cee38dc2b154a36ff61b99ef))
* **azure:** add redis resource ([#518](https://github.com/digdir/dialogporten/issues/518)) ([1b2c013](https://github.com/digdir/dialogporten/commit/1b2c0133477bb454edcec4158eb1de1c7f2b8de7))
* use redis in web api ([#527](https://github.com/digdir/dialogporten/issues/527)) ([eabd708](https://github.com/digdir/dialogporten/commit/eabd7085b12b23ffa85ba2ce0901e033c33f0e35))


### Bug Fixes

* Allow for 2 seconds clock skew in token validation ([#536](https://github.com/digdir/dialogporten/issues/536)) ([a0147b8](https://github.com/digdir/dialogporten/commit/a0147b8035dfdaf3ba157e3967a396f5ed897e8c))
* **azure:** rename connection string key for redis ([#533](https://github.com/digdir/dialogporten/issues/533)) ([db36213](https://github.com/digdir/dialogporten/commit/db36213f1cd4b5b306844c4a333a7556449f01fc))
* **azure:** revert to using connection string for IDistributedCache Redis ([#526](https://github.com/digdir/dialogporten/issues/526)) ([d19350d](https://github.com/digdir/dialogporten/commit/d19350d6b3f08efc473f9f7459dd9ef00db83f67))
* **azure:** use built-in policy for redis ([#521](https://github.com/digdir/dialogporten/issues/521)) ([2a8fa76](https://github.com/digdir/dialogporten/commit/2a8fa76761a92fe76b694f93cf263ce4336ba39d))
* **azure:** use secret uri instead of host name in app config ([#522](https://github.com/digdir/dialogporten/issues/522)) ([7cafd77](https://github.com/digdir/dialogporten/commit/7cafd77c50dcd131fa86575cc3a0cd34b08881ea))
* **azure:** use SSL port for redis in connection string ([#546](https://github.com/digdir/dialogporten/issues/546)) ([548bc47](https://github.com/digdir/dialogporten/commit/548bc47729e805ab078c9e158e234f860523166f))
* Change IfMatchDialogRevision to Revision in DTO ([#535](https://github.com/digdir/dialogporten/issues/535)) ([3a065d3](https://github.com/digdir/dialogporten/commit/3a065d3aecd4c0740dbd15cef3bd0792f865b667))
* purge should accept any content-type and no body ([#540](https://github.com/digdir/dialogporten/issues/540)) ([736fb59](https://github.com/digdir/dialogporten/commit/736fb59e511aa6b52683611af64e5c60bc224772))
* remove prefix for redis connection string ([#541](https://github.com/digdir/dialogporten/issues/541)) ([ceb204c](https://github.com/digdir/dialogporten/commit/ceb204c4196688f8f458f656dd4f8356a4cda488))
* Update Altinn Authorization integration ([#457](https://github.com/digdir/dialogporten/issues/457)) ([#469](https://github.com/digdir/dialogporten/issues/469)) ([d0d846d](https://github.com/digdir/dialogporten/commit/d0d846d6f7b9ee69a0dc501aafb8bae463ab14a1))

## [1.0.4](https://github.com/digdir/dialogporten/compare/v1.0.3...v1.0.4) (2024-02-29)


### Bug Fixes

* add extra comment in dockerfile ([#503](https://github.com/digdir/dialogporten/issues/503)) ([77541cb](https://github.com/digdir/dialogporten/commit/77541cba45ae914c73e07856c8654732a97e86e5))

## [1.0.3](https://github.com/digdir/dialogporten/compare/v1.0.2...v1.0.3) (2024-02-28)


### Bug Fixes

* remove whiteline in dockerfile ([9b14994](https://github.com/digdir/dialogporten/commit/9b149949cfb36a527acf62318acaeadb3dca3fd1))

## [1.0.2](https://github.com/digdir/dialogporten/compare/v1.0.1...v1.0.2) (2024-02-28)


### Bug Fixes

* always run staging dry-runs in release-please-pr ([3e390e7](https://github.com/digdir/dialogporten/commit/3e390e758d2c1797ab7e4ae36f151a17de072662))
* fix workflow permissions ([40e5485](https://github.com/digdir/dialogporten/commit/40e5485bd3aa39150e482e58de9cb03a2b347d03))
* fix workflow permissions ([b2213b2](https://github.com/digdir/dialogporten/commit/b2213b29e71d82e60cec3b55fa476d77aee1639f))
* **release-please:** use correct gh token ([#500](https://github.com/digdir/dialogporten/issues/500)) ([ebff656](https://github.com/digdir/dialogporten/commit/ebff65611ed5f94b1fe42a9a288d7a2b1644d906))
* use temporary gh token ([c1118ae](https://github.com/digdir/dialogporten/commit/c1118ae436854d3aabff59689ad1a8a342b97df5))

## [1.0.1](https://github.com/digdir/dialogporten/compare/v1.0.0...v1.0.1) (2024-02-28)


### Bug Fixes

* remove main-tag when tagging docker images ([#498](https://github.com/digdir/dialogporten/issues/498)) ([ddc1bad](https://github.com/digdir/dialogporten/commit/ddc1badd6fe71de9abacf7dd8dbd1714abf3ba11))

## 1.0.0 (2024-02-28)


### Features

* Add element count to eu list dto ([#414](https://github.com/digdir/dialogporten/issues/414)) ([934fa93](https://github.com/digdir/dialogporten/commit/934fa93b9b272684a160855a048c7fae3aa39f81))
* Add purge functionallity separate from soft delete. ([#483](https://github.com/digdir/dialogporten/issues/483)) ([1349efb](https://github.com/digdir/dialogporten/commit/1349efb8c81a4ba40703b49ad716116834f3180f))
* Add SeenBy per user ([#368](https://github.com/digdir/dialogporten/issues/368)) ([c68db9e](https://github.com/digdir/dialogporten/commit/c68db9eed984c175e90ecd29e1374cc9aeed1863))
* **azure:** parameterize SKUs ([#364](https://github.com/digdir/dialogporten/issues/364)) ([9c27c74](https://github.com/digdir/dialogporten/commit/9c27c744784ec29c95b969871e7807f96b288c03))
* change format of party identifier ([#376](https://github.com/digdir/dialogporten/issues/376)) ([27e6744](https://github.com/digdir/dialogporten/commit/27e674447331348b520a983fab0ea03e782afcaa)), closes [#220](https://github.com/digdir/dialogporten/issues/220)
* Container app revision verification on deploy ([#392](https://github.com/digdir/dialogporten/issues/392)) ([db13a89](https://github.com/digdir/dialogporten/commit/db13a8955e39b3012efd020cd62171a54a9b1ddb))
* Slack notifier IaC ([#341](https://github.com/digdir/dialogporten/issues/341)) ([80c3579](https://github.com/digdir/dialogporten/commit/80c35795278377089f0fe25248dfe8630fb358b7))


### Bug Fixes

* 412 status on multiple requests without revision header ([#427](https://github.com/digdir/dialogporten/issues/427)) ([047cf71](https://github.com/digdir/dialogporten/commit/047cf71945b60839d4de544041f42f1e6ff884f8))
* add APIM base uri for dialogporten ([948b9a4](https://github.com/digdir/dialogporten/commit/948b9a46ef503bf4171d80d50610da8606af37c2))
* add apim base uri for staging ([#451](https://github.com/digdir/dialogporten/issues/451)) ([580d946](https://github.com/digdir/dialogporten/commit/580d94604a089d689ab7783f5821481456b88de6))
* add base uri for web api ([#425](https://github.com/digdir/dialogporten/issues/425)) ([0aa941b](https://github.com/digdir/dialogporten/commit/0aa941bcb4a911ed1866e6c37c2c061af7db9ebd))
* add correct APIM base uri for dialogporten ([713771a](https://github.com/digdir/dialogporten/commit/713771a0248ed30a7336a09438f8f016bde106ef))
* add correct baseuri for altinn events ([#496](https://github.com/digdir/dialogporten/issues/496)) ([74940ab](https://github.com/digdir/dialogporten/commit/74940abfdc64a16a6c88bf432fbbf0725f3c4f5e))
* Add null checks, set lists to empty if null ([#434](https://github.com/digdir/dialogporten/issues/434)) ([f264aec](https://github.com/digdir/dialogporten/commit/f264aeca0e3eeee7e79356cbd1ae78107a1f1872))
* **azure:** fix postgresql auth config ([#357](https://github.com/digdir/dialogporten/issues/357)) ([4a4757f](https://github.com/digdir/dialogporten/commit/4a4757fa9a2e7e7cd6e958b7df222c10f99a388a))
* **azure:** remove default value for KEY_VAULT_SOURCE_KEYS ([#418](https://github.com/digdir/dialogporten/issues/418)) ([b0d74e8](https://github.com/digdir/dialogporten/commit/b0d74e81ba846e35c52f230f0cf7bc9078461607))
* **azure:** remove default values in params and ensure secure on params ([#415](https://github.com/digdir/dialogporten/issues/415)) ([94b9885](https://github.com/digdir/dialogporten/commit/94b98856b36df02fdf95237f0870127c919c38eb))
* **azure:** rename and fix outputs and pass correct secrets ([#416](https://github.com/digdir/dialogporten/issues/416)) ([68f0c8b](https://github.com/digdir/dialogporten/commit/68f0c8b06a0bacb23498ccd67b58bce4b116a79e))
* build errors for 8.0.200 ([#440](https://github.com/digdir/dialogporten/issues/440)) ([b133f8f](https://github.com/digdir/dialogporten/commit/b133f8fd686a73ed8a308a15941c5ddb9e2a5375))
* Check Content for null, use DependentRules, disallow empty localization values ([#413](https://github.com/digdir/dialogporten/issues/413)) ([894644a](https://github.com/digdir/dialogporten/commit/894644abee5e5d38fec3dcc1614bf13bc57efe13))
* Correct params for revision verification ([#405](https://github.com/digdir/dialogporten/issues/405)) ([4b98348](https://github.com/digdir/dialogporten/commit/4b98348c567b59347ad6c9aad4f19a4c6cd2a92d))
* Do not allow empty content ([#436](https://github.com/digdir/dialogporten/issues/436)) ([a083544](https://github.com/digdir/dialogporten/commit/a083544ed56911d89095abbfd0942c4333d0a1e8))
* do not prefix swagger document in development ([#491](https://github.com/digdir/dialogporten/issues/491)) ([e330ce3](https://github.com/digdir/dialogporten/commit/e330ce3998bc0fdb17b277abaa9aef27b5516361))
* remove path to swagger json ([fe1e770](https://github.com/digdir/dialogporten/commit/fe1e7706d55e55768899266a8a5f951a357358a5))
* rename migration job ([#423](https://github.com/digdir/dialogporten/issues/423)) ([3897db2](https://github.com/digdir/dialogporten/commit/3897db2e1b04c3aec70cb098abf6a2d6341ee750))
* restrict container apps to apim ip ([#448](https://github.com/digdir/dialogporten/issues/448)) ([1a1f3ad](https://github.com/digdir/dialogporten/commit/1a1f3ad9dbf12e7633e3ec89c4d3554109714b5e))
* Return 410 Gone when updating deleted dialog ([#464](https://github.com/digdir/dialogporten/issues/464)) ([2498b0a](https://github.com/digdir/dialogporten/commit/2498b0a6aa7c0901ba6d41ed95e99d66a5a6a0cd))
* set base path for swagger json ui ([476fdca](https://github.com/digdir/dialogporten/commit/476fdca40a1d59c3348be39cb8b97461aa57225d))
* set base url for swagger json ([#447](https://github.com/digdir/dialogporten/issues/447)) ([2161066](https://github.com/digdir/dialogporten/commit/2161066866ff124f6e1f3d7b11bac17de4da3d25))
* shorten secret name for container app job ([#422](https://github.com/digdir/dialogporten/issues/422)) ([09b2f30](https://github.com/digdir/dialogporten/commit/09b2f307bcc96f1cc2952c44cdd1c9e30c93c699))
* try echoing pgpassword in migration job🤫 ([#419](https://github.com/digdir/dialogporten/issues/419)) ([fe673a3](https://github.com/digdir/dialogporten/commit/fe673a37a35d409f4706dbc8de31c32373edcba2))
* Use data from events, not from db ([#455](https://github.com/digdir/dialogporten/issues/455)) ([469c606](https://github.com/digdir/dialogporten/commit/469c606f6a387a6cf16ee1567e2dc3d59d86373f))
