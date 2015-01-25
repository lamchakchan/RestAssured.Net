var restAssured = Require<RestAssured>();
restAssured
	.Given()
		.Name("storefront GET by GUID")
		.Header("Content-Type", "application/json")
		.Header("Accept-Encoding", "gzip,deflate")
	.When()
		.Get("http://qa.services.theknot.com/local-partners-search/v1/storefronts/E90A470B-8E5C-DF11-849B-0014C258F21E?apikey=ca7f6e91ee8134de9717707d86b29100")
	.Then()
		.Expect("id match", x => x.id == "E90A470B-8E5C-DF11-849B-0014C258F21E")
		.WriteAssertions();

restAssured
	.Given()
		.Name("storefront POST by Guid,long (display id)")
		.Header("Content-Type", "application/json")
		.Header("Accept-Encoding", "gzip,deflate")
		.Body("{ 'Id': ['E90A470B-8E5C-DF11-849B-0014C258F21E','314955'] }")
	.When()
		.Post("http://qa.services.theknot.com/local-partners-search/v1/storefronts?apikey=ca7f6e91ee8134de9717707d86b29100")
	.Then()
		.Expect("there are 2 profiles", x => x.profiles.Count == 2)
		.WriteAssertions();