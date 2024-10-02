# Changelog

## [1.20.1](https://github.com/digdir/dialogporten/compare/v1.20.0...v1.20.1) (2024-10-02)


### Bug Fixes

* Add separate settings for parties cache, don't cache invalid response from Altinn 2 ([#1194](https://github.com/digdir/dialogporten/issues/1194)) ([dbb79dc](https://github.com/digdir/dialogporten/commit/dbb79dc26cefc5f28c21a738f39199c36a49438f))

## [1.20.0](https://github.com/digdir/dialogporten/compare/v1.19.0...v1.20.0) (2024-09-30)


### Features

* **GraphQL:** Add DialogToken requirement for subscriptions ([#1124](https://github.com/digdir/dialogporten/issues/1124)) ([651ca62](https://github.com/digdir/dialogporten/commit/651ca62fdec02dec48b674b80acf52737036cf13))

## [1.19.0](https://github.com/digdir/dialogporten/compare/v1.18.1...v1.19.0) (2024-09-24)


### Features

* **breaking:** Move notification check endpoint to /actions ([#1175](https://github.com/digdir/dialogporten/issues/1175)) ([e0c1cf2](https://github.com/digdir/dialogporten/commit/e0c1cf205c66200f024431dc3392c988b99fdb30))


### Bug Fixes

* **janitor:** ensure Redis is configured correctly ([#1182](https://github.com/digdir/dialogporten/issues/1182)) ([37fe982](https://github.com/digdir/dialogporten/commit/37fe982ea9f08e48c75481008d614aaacf19a57d))

## [1.18.1](https://github.com/digdir/dialogporten/compare/v1.18.0...v1.18.1) (2024-09-23)


### Bug Fixes

* Add missing events to dialog subscription ([#1163](https://github.com/digdir/dialogporten/issues/1163)) ([162ce9a](https://github.com/digdir/dialogporten/commit/162ce9a9a0c4183d10e8edfe0f8c5589110b7a59))
* Fix BaseUri on localhost trailing slash discrepancy on OAuth metadata ([#1145](https://github.com/digdir/dialogporten/issues/1145)) ([09ce878](https://github.com/digdir/dialogporten/commit/09ce878cb537bf2e495e3801e0c769e25008246a))

## [1.18.0](https://github.com/digdir/dialogporten/compare/v1.17.0...v1.18.0) (2024-09-16)


### Features

* add dialogOpened activitytype ([#1110](https://github.com/digdir/dialogporten/issues/1110)) ([711fa6d](https://github.com/digdir/dialogporten/commit/711fa6dcbd3e8ab1240c765b9fe1b765f00fe86d))
* Add process and precedingProcess to dialog as optional fields ([#1092](https://github.com/digdir/dialogporten/issues/1092)) ([2bf0d30](https://github.com/digdir/dialogporten/commit/2bf0d30619f6c40716a70890cda47fa7b30ad0ac))


### Bug Fixes

* Allow setting UpdatedAt when creating Dialog ([#1105](https://github.com/digdir/dialogporten/issues/1105)) ([481e907](https://github.com/digdir/dialogporten/commit/481e907993a1c32337ea6a85ced8312ec4cd1e5b))
* Authorize access to dialog details for any mainresource action ([#1122](https://github.com/digdir/dialogporten/issues/1122)) ([a7e769a](https://github.com/digdir/dialogporten/commit/a7e769ad2be45c7f72169f7ae980ab24fd43ce72))

## [1.17.0](https://github.com/digdir/dialogporten/compare/v1.16.0...v1.17.0) (2024-09-10)


### Features

* Add SubjectResource entity and db migration ([#1048](https://github.com/digdir/dialogporten/issues/1048)) ([d04d764](https://github.com/digdir/dialogporten/commit/d04d764b80b855b4d906d23d48de53720b2d8bf1))
* **graphQL:** Add subscription for dialog details ([#1072](https://github.com/digdir/dialogporten/issues/1072)) ([8214acb](https://github.com/digdir/dialogporten/commit/8214acbf61085cacadaea3ef7e5f3d6ac222cc2c))
* Implement scalable dialog search authorization ([#875](https://github.com/digdir/dialogporten/issues/875)) ([aa8f84d](https://github.com/digdir/dialogporten/commit/aa8f84ded3aaf569e97e2d85f1035d1b14c59915))
* revise dialog status ([#1099](https://github.com/digdir/dialogporten/issues/1099)) ([0029f46](https://github.com/digdir/dialogporten/commit/0029f46c464e4ff05443cebb73be13b52879ab93))


### Bug Fixes

* ensure correct appsettings is used ([#1086](https://github.com/digdir/dialogporten/issues/1086)) ([d43f6d7](https://github.com/digdir/dialogporten/commit/d43f6d7d04108c8baaf131feb0b4a9c4efd18a42))
* ensure jobs are run with correct arguments and parameters ([#1085](https://github.com/digdir/dialogporten/issues/1085)) ([e21de56](https://github.com/digdir/dialogporten/commit/e21de56130c48684324fd648699a5965c8a88ebf))
* **webapi:** Return 422 when existing transmission IDs are used in dialog update ([#1094](https://github.com/digdir/dialogporten/issues/1094)) ([7a8a933](https://github.com/digdir/dialogporten/commit/7a8a933fd63f7c456f625bd1885ad429d1fc0832))

## [1.16.0](https://github.com/digdir/dialogporten/compare/v1.15.0...v1.16.0) (2024-09-04)


### Features

* **webapi:** Require legacy scope for HTML support ([#1073](https://github.com/digdir/dialogporten/issues/1073)) ([03237cc](https://github.com/digdir/dialogporten/commit/03237cc29d05f34dce3f683368117f546de40762))
* **webAPI:** Require UUIDv7  ([#1032](https://github.com/digdir/dialogporten/issues/1032)) ([e9b844f](https://github.com/digdir/dialogporten/commit/e9b844f8092bbb28c0ec1d63676593d78719954b))


### Bug Fixes

* Fix iss claim in dialog token ([#1047](https://github.com/digdir/dialogporten/issues/1047)) ([9ab4a85](https://github.com/digdir/dialogporten/commit/9ab4a85eea321fd80215616580785f4c99fa85bb))

## [1.15.0](https://github.com/digdir/dialogporten/compare/v1.14.0...v1.15.0) (2024-08-21)


### Features

* add support for serviceowner admin scope ([#1002](https://github.com/digdir/dialogporten/issues/1002)) ([2638b48](https://github.com/digdir/dialogporten/commit/2638b485f50ec7973aaf2fbdfb02ab07cb913f99))
* **web-api:** Add optional EndUserId param to ServiceOwner Get Dialog details API ([#1020](https://github.com/digdir/dialogporten/issues/1020)) ([1380b33](https://github.com/digdir/dialogporten/commit/1380b33f4b80cb25146a9785a174091a2db8465a))


### Bug Fixes

* **azure:** use correct ip for APIM in prod ([#1036](https://github.com/digdir/dialogporten/issues/1036)) ([fecc4c0](https://github.com/digdir/dialogporten/commit/fecc4c0b38d8c97413b10f5201749b9817ad6e31))

## [1.14.0](https://github.com/digdir/dialogporten/compare/v1.13.0...v1.14.0) (2024-08-19)


### Features

* **web-api:** add production config ([#1018](https://github.com/digdir/dialogporten/issues/1018)) ([689e7fe](https://github.com/digdir/dialogporten/commit/689e7fe2b087aac5609efddf146cf3b7f280a6fa))

## [1.13.0](https://github.com/digdir/dialogporten/compare/v1.12.1...v1.13.0) (2024-08-15)


### Features

* Add doc blocks on DTOs for OAS generation, CORS headers ([#987](https://github.com/digdir/dialogporten/issues/987)) ([01c34b8](https://github.com/digdir/dialogporten/commit/01c34b841c82b58fe96e0c0831c2dcb49902804e))
* **azure:** add bicep parameter files for production ([#1016](https://github.com/digdir/dialogporten/issues/1016)) ([7a7198a](https://github.com/digdir/dialogporten/commit/7a7198a6579ab2143a0a0250039f3ebbac6bf7b3))

## [1.12.1](https://github.com/digdir/dialogporten/compare/v1.12.0...v1.12.1) (2024-08-14)


### Bug Fixes

* **azure:** ensure environment parameter for production is correct ([#1014](https://github.com/digdir/dialogporten/issues/1014)) ([1612f9d](https://github.com/digdir/dialogporten/commit/1612f9dab192831b5214a8f2a3180b10100e24f5))

## [1.12.0](https://github.com/digdir/dialogporten/compare/v1.11.0...v1.12.0) (2024-08-14)


### Features

* Add current user flag to parties dto ([#993](https://github.com/digdir/dialogporten/issues/993)) ([e096743](https://github.com/digdir/dialogporten/commit/e0967436cea9f1efa8dca503e511dc66cf830591))
* Add notification condition check endpoint ([#965](https://github.com/digdir/dialogporten/issues/965)) ([f480ce0](https://github.com/digdir/dialogporten/commit/f480ce0733453864ce3bb2aa28d1fb4bba2655d2))


### Bug Fixes

* Using existing Transmission or Activity IDs should no longer result in internal server error on updates ([#980](https://github.com/digdir/dialogporten/issues/980)) ([0757b33](https://github.com/digdir/dialogporten/commit/0757b332e1194aee399b6a0ab7c6c66d5fbf037e))

## [1.11.0](https://github.com/digdir/dialogporten/compare/v1.10.0...v1.11.0) (2024-08-08)


### Features

* **azure:** scaffold ssh jumper ([#958](https://github.com/digdir/dialogporten/issues/958)) ([6228aa2](https://github.com/digdir/dialogporten/commit/6228aa2e543bb319ae8d8d0d097b19717b526896))


### Bug Fixes

* Correct the SeenLog list endpoints OpenAPI description ([#976](https://github.com/digdir/dialogporten/issues/976)) ([f6ebd19](https://github.com/digdir/dialogporten/commit/f6ebd19ee8ab790b3a7892776fc9e0be01004121))
* Using existing transmission/activity IDs should return HTTP 422 ([#960](https://github.com/digdir/dialogporten/issues/960)) ([01789b1](https://github.com/digdir/dialogporten/commit/01789b1f256b17445379194f3cf781a0d70fc1af)), closes [#959](https://github.com/digdir/dialogporten/issues/959)

## [1.10.0](https://github.com/digdir/dialogporten/compare/v1.9.0...v1.10.0) (2024-08-01)


### Features

* **azure:** add tags on azure applications ([#957](https://github.com/digdir/dialogporten/issues/957)) ([4081922](https://github.com/digdir/dialogporten/commit/4081922c47aa83fc95b6583f1d50ebf941d7e020))


### Bug Fixes

* **azure:** add product tag on all resources ([#955](https://github.com/digdir/dialogporten/issues/955)) ([6c76576](https://github.com/digdir/dialogporten/commit/6c76576397424e12feafa694b69a1a2e8bbd4d1f))

## [1.9.0](https://github.com/digdir/dialogporten/compare/v1.8.1...v1.9.0) (2024-07-30)


### Features

* **breaking:** Move front channel embeds to content ([#862](https://github.com/digdir/dialogporten/issues/862)) ([c9b50e9](https://github.com/digdir/dialogporten/commit/c9b50e9ea7022c5bf22b472cf8859fd6faf66df6))
* **breaking:** Remove DialogElements, add Attachments ([#867](https://github.com/digdir/dialogporten/issues/867)) ([dbe296a](https://github.com/digdir/dialogporten/commit/dbe296aa3f25e88227109ca604efd81616f2b4ab))
* **breaking:** Remove PUT/DELETE endpoints for DialogElements ([#844](https://github.com/digdir/dialogporten/issues/844)) ([51eb898](https://github.com/digdir/dialogporten/commit/51eb89832f56081aa4b3eb2d30b7d19b1fb8f217))
* **breaking:** Rename CultureCode to LanguageCode ([#871](https://github.com/digdir/dialogporten/issues/871)) ([96d50fc](https://github.com/digdir/dialogporten/commit/96d50fc40b075b31d342b8dc27e82924c30e9b83))
* **breaking:** Renaming dialog activity types ([#919](https://github.com/digdir/dialogporten/issues/919)) ([af262b1](https://github.com/digdir/dialogporten/commit/af262b146ce78cd7eed6325d0a3ac8d662000107))
* Change content array to object with properties for each content type ([#905](https://github.com/digdir/dialogporten/issues/905)) ([d549f19](https://github.com/digdir/dialogporten/commit/d549f194903d5a48e0563c1ceb942db2d333dd59))
* Implement actor entity ([#912](https://github.com/digdir/dialogporten/issues/912)) ([a635fcb](https://github.com/digdir/dialogporten/commit/a635fcb04e988c416ae98928af05934ddd187de1))
* Introduce Transmissions ([#932](https://github.com/digdir/dialogporten/issues/932)) ([3ca495f](https://github.com/digdir/dialogporten/commit/3ca495f0c900862c1a0bbf4b7acd8350f7649347))
* Rename DialogStatus enum values ([#915](https://github.com/digdir/dialogporten/issues/915)) ([5aea32b](https://github.com/digdir/dialogporten/commit/5aea32b7299bd459fa529ada197fe42817d3aed7))
* **WebAPI:** Add Transmission endpoints  ([#943](https://github.com/digdir/dialogporten/issues/943)) ([d608ade](https://github.com/digdir/dialogporten/commit/d608adebadc12d099c04c4b174569392837603e6))


### Bug Fixes

* Allow new activities to reference old activities ([#935](https://github.com/digdir/dialogporten/issues/935)) ([bbc443e](https://github.com/digdir/dialogporten/commit/bbc443e121ee5121bac6b9fe92ee3296bb45218f))
* **auth:** Malformed JWTs no longer results in InternalServerError  ([#870](https://github.com/digdir/dialogporten/issues/870)) ([5f2f386](https://github.com/digdir/dialogporten/commit/5f2f386dabfa3b25cc56370f7e99a02fa566d5e3))
* **slackNotifier:** Add missing deployment of Slack notifier function in staging environment  ([#861](https://github.com/digdir/dialogporten/issues/861)) ([59091f7](https://github.com/digdir/dialogporten/commit/59091f790c52fc7cae9b66f98b71c6db8e4bd9d3))
* Update e2e tests for actor model ([#918](https://github.com/digdir/dialogporten/issues/918)) ([ec1fcb1](https://github.com/digdir/dialogporten/commit/ec1fcb1c094b16a5a13b90350b7e2a58feaf9b82))
* **WebAPI:** Allow purging of softly deleted dialogs ([#940](https://github.com/digdir/dialogporten/issues/940)) ([c527c9f](https://github.com/digdir/dialogporten/commit/c527c9f0db7a5ad2136302de69aaec55b860fcc6))

## [1.8.1](https://github.com/digdir/dialogporten/compare/v1.8.0...v1.8.1) (2024-06-12)


### Bug Fixes

* **azure:** fix redis deployment ([#847](https://github.com/digdir/dialogporten/issues/847)) ([23781e6](https://github.com/digdir/dialogporten/commit/23781e65e5bd567f84e728ed3f808d10b2c904c5))

## [1.8.0](https://github.com/digdir/dialogporten/compare/v1.7.1...v1.8.0) (2024-06-12)


### Features

* Add support for external resource references in authorizationAttributes ([#801](https://github.com/digdir/dialogporten/issues/801)) ([1e674bd](https://github.com/digdir/dialogporten/commit/1e674bdd3a133fb73d9a5418822486d1c26d32de))
* Add user types ([#768](https://github.com/digdir/dialogporten/issues/768)) ([b6fd439](https://github.com/digdir/dialogporten/commit/b6fd439e8865e3eeec9470172abe7117ed948ee4))
* Front channel embeds ([#792](https://github.com/digdir/dialogporten/issues/792)) ([c3000bd](https://github.com/digdir/dialogporten/commit/c3000bdbc546670e1001d608c0f7541e7af64187))
* GUI actions without navigation ([#785](https://github.com/digdir/dialogporten/issues/785)) ([f2d9136](https://github.com/digdir/dialogporten/commit/f2d91364f708139be3c23b3be26ef95092675824))
* Remove IsBackChannel concept from GUI Actions ([#819](https://github.com/digdir/dialogporten/issues/819)) ([18101c1](https://github.com/digdir/dialogporten/commit/18101c1efa6854e3720d6335490b2142933400f3))
* Rename IsDeleteAction to IsDeleteDialogAction ([#820](https://github.com/digdir/dialogporten/issues/820)) ([18a1f6e](https://github.com/digdir/dialogporten/commit/18a1f6e2f3ac3d4d322e269a58780fa922a9f400))
* **schema:** Rename MimeType to MediaType ([#813](https://github.com/digdir/dialogporten/issues/813)) ([6490625](https://github.com/digdir/dialogporten/commit/64906258a9880899d086b16406c0a8ae85ffd073))
* **schema:** undo setting performed by if not set ([#802](https://github.com/digdir/dialogporten/issues/802)) ([c19f47a](https://github.com/digdir/dialogporten/commit/c19f47a4d1a018e7ba2ad1802ff62bc7b27f7b11))


### Bug Fixes

* remove maskinporten aux from config ([#827](https://github.com/digdir/dialogporten/issues/827)) ([2e4e81a](https://github.com/digdir/dialogporten/commit/2e4e81a2984dc23d91470d959804eb617dd63f1a))
* **schema:** add package-lock file ([#804](https://github.com/digdir/dialogporten/issues/804)) ([987dfa1](https://github.com/digdir/dialogporten/commit/987dfa170f38caf6488979e883578f43319b6cb9))

## [1.7.1](https://github.com/digdir/dialogporten/compare/v1.7.0...v1.7.1) (2024-05-31)


### Bug Fixes

* **ci:** separate migration-job deployments ([#795](https://github.com/digdir/dialogporten/issues/795)) ([c7f5dba](https://github.com/digdir/dialogporten/commit/c7f5dba8842ed373b8a7b9a8f6f75c02fc3a3f2c))

## [1.7.0](https://github.com/digdir/dialogporten/compare/v1.6.2...v1.7.0) (2024-05-30)


### Features

* Change party identifier separator to single colon ([#746](https://github.com/digdir/dialogporten/issues/746)) ([3342703](https://github.com/digdir/dialogporten/commit/3342703cbbfda501c79389f09ce2b5b8aeb19ae9))
* Correspondence dialog type  ([#692](https://github.com/digdir/dialogporten/issues/692)) ([317a213](https://github.com/digdir/dialogporten/commit/317a2137298f0034e69c2a43828c70f270c958c7))


### Bug Fixes

* Fix broken source URL in cloud events ([#753](https://github.com/digdir/dialogporten/issues/753)) ([4a45eda](https://github.com/digdir/dialogporten/commit/4a45eda8718a2a2258b61eaf768dda88a732a647))
* **graphql:** Add missing enum value ExtendedStatus in schema ([#733](https://github.com/digdir/dialogporten/issues/733)) ([8670595](https://github.com/digdir/dialogporten/commit/8670595bbe5a5eb8c0512cac6f92628bb0aac594))
* **graphql:** Make OrderBy nullable ([#741](https://github.com/digdir/dialogporten/issues/741)) ([3ae72ce](https://github.com/digdir/dialogporten/commit/3ae72cebab5efd041defdfabedaf70a9090d80b8))
* Update to new scope ([#750](https://github.com/digdir/dialogporten/issues/750)) ([d6fb439](https://github.com/digdir/dialogporten/commit/d6fb4398d56eb454195188a5f2fa64736e689567))
* **webapi:** Fix Swagger URL for new APIM ([#755](https://github.com/digdir/dialogporten/issues/755)) ([2388d54](https://github.com/digdir/dialogporten/commit/2388d5491f7ae1f9166e3ca84b07cf47e887dadd))

## [1.6.2](https://github.com/digdir/dialogporten/compare/v1.6.1...v1.6.2) (2024-05-10)


### Bug Fixes

* **gql:** Add missing graphQl appsettings for staging ([#714](https://github.com/digdir/dialogporten/issues/714)) ([97b7da6](https://github.com/digdir/dialogporten/commit/97b7da6e1b817c492467cec587513a2b4a00e518))
* Use correct scope for authorization API for remaining runtimes ([#711](https://github.com/digdir/dialogporten/issues/711)) ([0691f36](https://github.com/digdir/dialogporten/commit/0691f360b6f28cc353279b9e65c4de7e53d113e4))

## [1.6.1](https://github.com/digdir/dialogporten/compare/v1.6.0...v1.6.1) (2024-05-08)


### Bug Fixes

* Use correct scope for authorization API ([#709](https://github.com/digdir/dialogporten/issues/709)) ([38253ad](https://github.com/digdir/dialogporten/commit/38253ad1128745f54ef5d2bd4393c2d1efdae58e))

## [1.6.0](https://github.com/digdir/dialogporten/compare/v1.5.0...v1.6.0) (2024-05-07)


### Features

* Add authorization caching ([#591](https://github.com/digdir/dialogporten/issues/591)) ([2f86d7e](https://github.com/digdir/dialogporten/commit/2f86d7e6982f2d1e228a753a0b625904caf443ea))
* Add GraphQL POC ([#636](https://github.com/digdir/dialogporten/issues/636)) ([c779eac](https://github.com/digdir/dialogporten/commit/c779eac72372e925f8bb3d348812322ebdef319a))
* Add support for apps as serviceresource ([#658](https://github.com/digdir/dialogporten/issues/658)) ([adf91ce](https://github.com/digdir/dialogporten/commit/adf91ce739980abd0177b26d55f9db956f1e95fb))
* Authorized parties endpoint in enduser API ([#661](https://github.com/digdir/dialogporten/issues/661)) ([050ccbb](https://github.com/digdir/dialogporten/commit/050ccbb548d9f8bf03842041bd675f8f27504141))


### Bug Fixes

* Accept app references with urn:altinn:resource prefix ([#685](https://github.com/digdir/dialogporten/issues/685)) ([c9a5606](https://github.com/digdir/dialogporten/commit/c9a5606f932fb029ecf8a891b9a9246dc631db52))
* ensure performed by is set for activities ([#628](https://github.com/digdir/dialogporten/issues/628)) ([1adf075](https://github.com/digdir/dialogporten/commit/1adf075de2a25fa83b4909cf9b29020b53cf9c0f))
* Use HttpClient wrappers that ensure success to match FusionCache expectations ([#684](https://github.com/digdir/dialogporten/issues/684)) ([7c1e966](https://github.com/digdir/dialogporten/commit/7c1e96650cf54594a33fb380c8b42518392f1741))

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
