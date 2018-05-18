var QueryBuilder = {
    BaseUrl: "",
    LastRunQuery: "",
    LastRunPage: 1,
    LastUpdatedQueryId: 0,
    LoadOperators: function (fieldId, statementObj) {
        $.ajax({
            url: QueryBuilder.BaseUrl + "Query/GetFieldOperatorList",
            type: "POST",
            data: { fieldId: fieldId },
            success: function (data) {
                var list = "";
                $(data).each(function () {
                    list += "<option value='" + this.OpId + "'>" + this.Symbol.toUpperCase() + "</option>"
                });
                statementObj.find('.operator ').html(list).show();
                statementObj.find('.searchparam1').show();
                statementObj.find('.searchparam2').hide();
            }
        })
    },

    RunQuery: function (query, page) {
        $('.query-results').hide();
        this.LastRunQuery = query;
        this.LastRunPage = page;
        $.ajax({
            url: QueryBuilder.BaseUrl + "ElasticSearch/QueryBuilderSearch",
            contentType: 'application/json',
            type: "POST",
            data: JSON.stringify({ query: query, page: page }),
            success: function (data) {
                QueryBuilder.DisplayResults(data);
            }
        })
    },

    SaveQuery: function (query) {
        $.ajax({
            url: QueryBuilder.BaseUrl + "ElasticSearch/QueryBuilderSave",
            contentType: 'application/json',
            type: "POST",
            data: JSON.stringify({ query: query }),
            success: function (data) {
                QueryBuilder.DisplaySavedQuery(data.QueryId, query);
            }
        })
    },

    UpdateQuery: function () {

        var Query = {};
        var users = [];
        Query.QueryId = QueryBuilder.LastUpdatedQueryId;
        Query.Name = $(".update-query-name").val();
        Query.ColorCode = $(".update-color-code").val();
        Query.Public = $('.public-update-check').is(':checked');

        //Users
        if ($('#multiple-user-select-update option:selected').length > 0) {
            $('#multiple-user-select-update option:selected').each(function () {
                users.push({
                    UserId: $(this).val()
                });
            });
            Query.UserProfiles = users;
        }

        $.ajax({
            url: QueryBuilder.BaseUrl + "ElasticSearch/QueryBuilderUpdate",
            contentType: 'application/json',
            type: "POST",
            data: JSON.stringify({ query: Query }),
            success: function (data) {
                QueryBuilder.UpdateSavedQuery(data);
            }
        })

    },

    DeleteQuery: function (query_id) {
        $.ajax({
            url: QueryBuilder.BaseUrl + "Query/DeleteQuery",
            type: "POST",
            data: { id: query_id },
            success: function (data) { }
        })
    },

    UnWatchQuery: function (query_id) {
        $.ajax({
            url: QueryBuilder.BaseUrl + "Query/UnwatchQuery",
            type: "POST",
            data: { id: query_id },
            success: function (data) { }
        })
    },

    LoadSavedQueryDetails: function (query_id) {
        $.ajax({
            url: QueryBuilder.BaseUrl + "Query/LoadSavedQueryDetails",
            type: "POST",
            data: { id: query_id },
            success: function (data) {
                QueryBuilder.DisplaySavedQueryDetails(data);
            }
        })
    },

    DisplaySavedQueryDetails: function (details) {
        console.log(details.Name);
        $(".update-query-name").val(details.Name);
        $("#cp3").colorpicker('setValue', details.Color)
        $(".update-query-id").val(details.QueryId)

        if ($(".public-update-check").is(":checked")) {
            $(".public-update-check").trigger("click");
            $("#multiple-user-select-update").multiselect('deselectAll', false).multiselect('refresh');
        }

        if (details.Users.length > 0) {
            $(".public-update-check").trigger("click");
            $.each(details.Users, function () {
                $("#multiple-user-select-update").multiselect('select', this);
            });
        }
        $("#updateQueryModal").modal("show");
    },

    DisplaySavedQuery: function (query_id, query) {
        var savedQuery = $('.saved-query-prototype').children().clone();
        savedQuery.find('.div-color-code').css("background-color", query.ColorCode);
        savedQuery.find('.load-query-id').val(query_id);
        savedQuery.find('.delete-query-id').val(query_id);
        savedQuery.find('.panel-heading').html(query.Name);
        $('.query-list').append(savedQuery.hide());
        savedQuery.fadeIn();
    },

    UpdateSavedQuery: function (query) {
        var parentPanel = $(".load-query-id[value='" + query.QueryId + "']").parents(".saved-query");
        parentPanel.find('.div-color-code').css("background-color", query.ColorCode);
        parentPanel.find('.panel-heading').html(query.Name);
    },

    DisplayResults: function (data) {
        $('.query-results').fadeIn();

        //Add Total
        var span = $("<h4>").html("Total documents found: " + data.Total);
        $('.query-results-body').html("").append(span);

        //Drop Down
        var total_pages = Math.ceil(data.Total / 10);
        var optionlist = "";
        for (var i = 1; i <= total_pages; i++) {
            if (i == this.LastRunPage) {
                optionlist += "<option selected>";
            } else {
                optionlist += "<option>";
            }
            optionlist += i + "</option>";
        }
        var page_select = $("<select id='page_number' class='form-control'>").append(optionlist);
        $('.query-results-body').append(page_select).append("<br />");
        //Create Result Table
        var table = $('<table class="table table-striped table-bordered table-hover">');
        //Headers
        var headers = $("<tr>").append("<th>File Name</th><th>File Extension</th><th>Author</th><th>Page Count</th><th>Score</th><th>Download</th>")
        table.append(headers);
        var documents = data.Documents;
        var hits = data.Hits;
        //Body
        for (var i = 0; i < documents.length; i++) {
            var row = $("<tr>").append("<td>" + documents[i].File.FileName + "</td><td>" + documents[i].File.Extension + "</td><td>" + documents[i].Meta.Author + "</td><td>" + documents[i].Meta.Raw.PageCount + "</td><td>" + hits[i].Score + "</td><td><a href='DownloadFile?esVirtualPath=" + encodeURIComponent(documents[i].Path.Virtual) + "' class='btn btn-success'><i class='fa fa-download' aria-hidden='true'></i></a></td>");
            table.append(row);
        }
        $('.query-results-body').append(table);

        //Highlights Table    
        for (var i = 0; i < hits.length; i++) {
            var highlights_table = $('<table class="table table-striped table-bordered">');
            var header = $("<tr>").append("<th>Highlights for File Name: " + documents[i].File.FileName + "</th>")
            highlights_table.append(header);
            if (typeof hits[i].Highlights.content != "undefined") {
                var highlights = hits[i].Highlights.content.Highlights;
                for (var j = 0; j < highlights.length; j++) {
                    var row = $("<tr>").append("<td>" + highlights[j] + "</td>");
                    highlights_table.append(row);
                }
            }
            $('.query-results-body').append(highlights_table);
        }
    },

    BuildQuery: function () {
        var query = {};
        var statements = [];
        var hasErrors = 0;
        $(".statements").find(".statement").each(function () {
            var statement = {};
            //Query Operator
            if ($(this).find(".sql-and-operator").is(":visible")) {
                statement.QueryOperatorOpId = $(this).find(".sql-and-operator").data("value");
            }

            if ($(this).find(".sql-or-operator").is(":visible")) {
                statement.QueryOperatorOpId = $(this).find(".sql-or-operator").data("value");
            }

            //Query Field
            statement.FieldId = $(this).find(".field").val();

            //Query Field Operator
            if (!$(this).find(".operator").is(":visible")) {
                $("#formatErrorModal").modal("show");
                hasErrors = 1;
                return false;
            }
            else {
                statement.OperatorOpId = $(this).find(".operator").val();
            }

            //Query Search Param
            statement.SearchParam1 = $(this).find(".searchparam1").val();
            statement.SearchParam2 = $(this).find(".searchparam2").val();

            statements.push(statement)

        });
        if (hasErrors) {
            return null;
        }
        query.Statements = statements;
        query.Name = $('.query-name').val();
        query.ColorCode = $('.color-code').val();
        query.Public = $('.public-check').is(':checked');

        //Users
        if ($('#multiple-user-select option:selected').length > 0) {
            var users = [];
            $('#multiple-user-select option:selected').each(function () {
                users.push({
                    UserId: $(this).val()
                });
            });
            query.UserProfiles = users;
        }
        return query;
    }
}

/*-- Events -- */

$(document).ready(function () {
    $('#multiple-user-select').multiselect();
    $('#multiple-user-select-update').multiselect();
    $('#cp2').colorpicker();
    $('#cp3').colorpicker();
    $('[data-toggle="popover"]').popover();
});


$(document).on("click", ".and-op", function () {
    //clone prototype statement
    var statement = $(".statement-prototype").children().clone();
    //unhide delete button
    statement.find(".delete-statement").css('display', 'inline-block');
    statement.find(".sql-and-operator").css('display', 'inline-block');
    $(".statements").append(statement);
});

$(document).on("click", ".or-op", function () {
    //clone prototype statement
    var statement = $(".statement-prototype").children().clone();
    //unhide delete button
    statement.find(".delete-statement").css('display', 'inline-block');
    statement.find(".sql-or-operator").css('display', 'inline-block');
    $(".statements").append(statement);
});

$(document).on("click", ".delete-statement", function () {
    $(this).parents(".statement").remove();
});

$(document).on("change", ".field", function () {
    QueryBuilder.LoadOperators($(this).val(), $(this).parents(".statement"));
});

$(document).on("change", ".operator", function () {
    var statement = $(this).parents(".statement");
    if ($(this).val() == "7") {
        statement.find(".searchparam2").show();
    } else {
        statement.find(".searchparam2").hide();
    }
});

$(document).on("click", ".run", function () {
    var query = QueryBuilder.BuildQuery();
    if (query != null) {
        QueryBuilder.RunQuery(query, 1);
    }
});

$(document).on("change", "#page_number", function () {
    var query = QueryBuilder.BuildQuery();
    if (query != null) {
        QueryBuilder.RunQuery(query, $(this).val());
    }
});

$(document).on("click", ".save", function () {
    $("#saveQueryModal").modal("show");
});

$(document).on("click", ".confirm-save", function () {
    var query = QueryBuilder.BuildQuery();
    if (query != null) {
        QueryBuilder.SaveQuery(query);
    }
    $('.query-name').val("");
});

$(document).on("click", ".confirm-update", function () {

    QueryBuilder.UpdateQuery();
});

$(document).on("click", ".delete-saved-query", function () {
    var query_id = $(this).siblings('.delete-query-id').val();
    QueryBuilder.DeleteQuery(query_id);
    $(this).parents('.saved-query').fadeOut();

});

$(document).on("click", ".edit-saved-query", function () {
    var query_id = $(this).siblings('.delete-query-id').val();
    QueryBuilder.LastUpdatedQueryId = query_id;
    QueryBuilder.LoadSavedQueryDetails(query_id);
});

$(document).on("click", ".unwatch-query", function () {
    var query_id = $(this).siblings('.delete-query-id').val();
    QueryBuilder.UnWatchQuery(query_id);
    $(this).parents('.saved-query').fadeOut();
});

$(document).on("click", ".public-check", function () {
    if ($(this).is(":checked")) {
        $('#multiple-user-select').multiselect("enable");
    } else {
        $('#multiple-user-select').multiselect("disable");
        $("#multiple-user-select-update").multiselect('deselectAll', false).multiselect('refresh');
    }

});

$(document).on("click", ".public-update-check", function () {
    if ($(this).is(":checked")) {
        $('#multiple-user-select-update').multiselect("enable");
    } else {
        $('#multiple-user-select-update').multiselect("disable");
        $("#multiple-user-select-update").multiselect('deselectAll', false).multiselect('refresh');
    }

});