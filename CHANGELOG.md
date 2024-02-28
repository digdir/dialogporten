# Changelog

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
