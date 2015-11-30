
        <script>
            $('#EventCategoryChart').highcharts({
                chart: {
                    plotBackgroundColor: null,
                    plotBorderWidth: null,
                    plotShadow: false,
                    type: 'pie'
                },
                title: {
                    text: 'Event Created Based on Category'
                },
                tooltip: {
                    pointFormat: '{series.name}: {point.y} event(s) <b>{point.percentage:.1f}%</b>'
                },
                plotOptions: {
                    pie: {
                        allowPointSelect: true,
                        cursor: 'pointer',
                        dataLabels: {
                            enabled: false
                        },
                        showInLegend: true
                    }
                },
                series: [
                    {
                        name: 'Category',
                        colorByPoint: true,
                        data: [
                            @foreach (var item in Model.EventCountStatistic)
                    {
                        <text>{name:'</text>@item.Key<text>',</text>
                        <text>y: </text>
                        @item.Value
                        <text>},</text>
                    }
                        ]
                    }
                ]
            });
        </script>
        <script>
            $('#EventCreatedEachMonth').highcharts({
                chart: {
                    type: 'column'
                },
                title: {
                    text: 'Event Created Each Month'
                },
                xAxis: {
                    categories: [
                        'Jan',
                        'Feb',
                        'Mar',
                        'Apr',
                        'May',
                        'Jun',
                        'Jul',
                        'Aug',
                        'Sep',
                        'Oct',
                        'Nov',
                        'Dec'
                    ],
                    crosshair: true
                },
                yAxis: {
                    min: 0,
                    title: {
                        text: 'Events'
                    }
                },
                tooltip: {
                    headerFormat: '<span style="font-size:10px">{point.key}</span><table>',
                    pointFormat: '<tr><td style="color:{series.color};padding:0">{series.name}: </td>' +
                        '<td style="padding:0"><b>{point.y} event(s)</b></td></tr>',
                    footerFormat: '</table>',
                    shared: true,
                    useHTML: true
                },
                plotOptions: {
                    column: {
                        pointPadding: 0.2,
                        borderWidth: 0
                    }
                },
                series: [
                    @foreach (var item in Model.EventCreatedEachMonth)
            {
                <text>{name: '</text>@item.Key<text>',</text>
                <text>data: [ </text>
                foreach (var number in @item.Value)
                {
                    @number<text>,</text>
                }
                <text>]},</text>
            }
                ]
            });
        </script>
<script>
    $('#TopTenEvents').highcharts({
        chart: {
            type: 'bar'
        },
        title: {
            text: 'Top Ten Event Based on Interactions'
        },
        xAxis: {
            categories: [
                @foreach (var item in Model.TopTenEvents)
                {
                    <text>'</text>@item.Key.EventName<text>',</text>
                }
            ],
            title: {
                text: null
            }
        },
        yAxis: {
            min: 0,
            title: {
                text: 'Ranks',
                align: 'high'
            },
            labels: {
                overflow: 'justify'
            }
        },
        tooltip: {
            valueSuffix: ' Points'
        },
        plotOptions: {
            bar: {
                dataLabels: {
                    enabled: true
                }
            }
        },
        legend: {
            layout: 'vertical',
            align: 'right',
            verticalAlign: 'top',
            x: -40,
            y: 80,
            floating: true,
            borderWidth: 1,
            backgroundColor: ((Highcharts.theme && Highcharts.theme.legendBackgroundColor) || '#FFFFFF'),
            shadow: true
        },
        credits: {
            enabled: false
        },
        series: [
            {
                name: 'Points',
                data: [
                    @foreach (var item in Model.TopTenEvents)
                    {<text></text>@item.Value<text>,</text>
                    }
                ]
            }
        ]
    });
</script>
<script>
    $('#EventByStatus').highcharts({
        chart: {
            plotBackgroundColor: null,
            plotBorderWidth: null,
            plotShadow: false,
            type: 'pie'
        },
        title: {
            text: 'Event By Status'
        },
        tooltip: {
            pointFormat: '{series.name}: {point.y} event(s) <b>{point.percentage:.1f}%</b>'
        },
        plotOptions: {
            pie: {
                allowPointSelect: true,
                cursor: 'pointer',
                dataLabels: {
                    enabled: false
                },
                showInLegend: true
            }
        },
        series: [
            {
                name: 'Event',
                colorByPoint: true,
                data: [
                    @foreach (var item in Model.EventByStatus)
                    {
                        <text>{name:'</text>@item.Key<text>',</text>
                        <text>y: </text>
                        @item.Value
                        <text>},</text>
                    }
                ]
            }
        ]
    });
</script>

        <script>
            $('#TopTenLocations').highcharts({
                chart: {
                    type: 'bar'
                },
                title: {
                    text: 'Top Ten Locations Having Most Event'
                },
                xAxis: {
                    categories: [
                        @foreach (var item in Model.TopTenLocations)
                {
                    <text>'</text>@item.Key.LocationName<text>',</text>
                }
                    ],
                    title: {
                        text: null
                    }
                },
                yAxis: {
                    min: 0,
                    title: {
                        text: 'Events',
                        align: 'high'
                    },
                    labels: {
                        overflow: 'justify'
                    }
                },
                tooltip: {
                    valueSuffix: ' event(s)'
                },
                plotOptions: {
                    bar: {
                        dataLabels: {
                            enabled: true
                        }
                    }
                },
                legend: {
                    layout: 'vertical',
                    align: 'right',
                    verticalAlign: 'top',
                    x: -40,
                    y: 80,
                    floating: true,
                    borderWidth: 1,
                    backgroundColor: ((Highcharts.theme && Highcharts.theme.legendBackgroundColor) || '#FFFFFF'),
                    shadow: true
                },
                credits: {
                    enabled: false
                },
                series: [
                {
                    name: 'event(s)',
                    data: [
                        @foreach (var item in Model.TopTenLocations)
                {<text></text>@item.Value<text>,</text>
                }
                    ]
                }
                ]
            });
        </script>
<script>
    $('#TopTenUser').highcharts({
        chart: {
            type: 'bar'
        },
        title: {
            text: 'Top Ten Users Having Most Event'
        },
        xAxis: {
            categories: [
                @foreach (var item in Model.TopTenUser)
                {
                    <text>'</text>@item.Key.UserName<text>',</text>
                }
            ],
            title: {
                text: null
            }
        },
        yAxis: {
            min: 0,
            title: {
                text: 'Users',
                align: 'high'
            },
            labels: {
                overflow: 'justify'
            }
        },
        tooltip: {
            valueSuffix: ' event(s)'
        },
        plotOptions: {
            bar: {
                dataLabels: {
                    enabled: true
                }
            }
        },
        legend: {
            layout: 'vertical',
            align: 'right',
            verticalAlign: 'top',
            x: -40,
            y: 80,
            floating: true,
            borderWidth: 1,
            backgroundColor: ((Highcharts.theme && Highcharts.theme.legendBackgroundColor) || '#FFFFFF'),
            shadow: true
        },
        credits: {
            enabled: false
        },
        series: [
        {
            name: 'event(s)',
            data: [
                @foreach (var item in Model.TopTenUser)
                {<text></text>@item.Value<text>,</text>
                }
            ]
        }
        ]
    });
</script>
<script>
    $('#GenderRate').highcharts({
        chart: {
            plotBackgroundColor: null,
            plotBorderWidth: null,
            plotShadow: false,
            type: 'pie'
        },
        title: {
            text: 'Gender Rate'
        },
        tooltip: {
            pointFormat: '{series.name}: {point.y} person(s) <b>{point.percentage:.1f}%</b>'
        },
        plotOptions: {
            pie: {
                allowPointSelect: true,
                cursor: 'pointer',
                dataLabels: {
                    enabled: false
                },
                showInLegend: true
            }
        },
        series: [
            {
                name: 'Person',
                colorByPoint: true,
                data: [
                    @foreach (var item in Model.GenderRate)
                    {
                        <text>{name:'</text>@item.Key<text>',</text>
                        <text>y: </text>
                        @item.Value
                        <text>},</text>
                    }
                ]
            }
        ]
    });
</script>
<script>
    $('#GroupAge').highcharts({
        chart: {
            type: 'column'
        },
        title: {
            text: 'Group Age'
        },

        xAxis: {
            categories: [
                'Group Age'
            ],
            crosshair: true
        },
        yAxis: {
            min: 0,
            title: {
                text: 'Person(s)'
            }
        },
        tooltip: {
            headerFormat: '<span style="font-size:10px">{point.key}</span><table>',
            pointFormat: '<tr><td style="color:{series.color};padding:0">{series.name}: </td>' +
                '<td style="padding:0"><b>{point.y} person(s)</b></td></tr>',
            footerFormat: '</table>',
            shared: true,
            useHTML: true
        },
        plotOptions: {
            column: {
                pointPadding: 0.2,
                borderWidth: 0
            }
        },
        series: [
            @foreach (var item in Model.GroupbyAge)
            {
                <text>{name: '</text>@item.Key<text>',</text>
                <text>data: [ </text>
                    @item.Value<text>,</text>
                <text>]},</text>
            }
        ]
    });
</script>
<script>
    $('#ReportByType').highcharts({
        chart: {
            plotBackgroundColor: null,
            plotBorderWidth: null,
            plotShadow: false,
            type: 'pie'
        },
        title: {
            text: 'Reports by Type'
        },
        tooltip: {
            pointFormat: '{series.name}: {point.y} report(s) <b>{point.percentage:.1f}%</b>'
        },
        plotOptions: {
            pie: {
                allowPointSelect: true,
                cursor: 'pointer',
                dataLabels: {
                    enabled: false
                },
                showInLegend: true
            }
        },
        series: [
            {
                name: 'Report',
                colorByPoint: true,
                data: [
                    @foreach (var item in Model.ReportByType)
                    {
                        <text>{name:'</text>@item.Key<text>',</text>
                        <text>y: </text>
                        @item.Value
                        <text>},</text>
                    }
                ]
            }
        ]
    });
</script>
<script>
    $('#ReportByStatus').highcharts({
        chart: {
            plotBackgroundColor: null,
            plotBorderWidth: null,
            plotShadow: false,
            type: 'pie'
        },
        title: {
            text: 'Report By Status'
        },
        tooltip: {
            pointFormat: '{series.name}: {point.y} report(s) <b>{point.percentage:.1f}%</b>'
        },
        plotOptions: {
            pie: {
                allowPointSelect: true,
                cursor: 'pointer',
                dataLabels: {
                    enabled: false
                },
                showInLegend: true
            }
        },
        series: [
            {
                name: 'Report',
                colorByPoint: true,
                data: [
                    @foreach (var item in Model.ReportByStatus)
                    {
                        <text>{name:'</text>@item.Key<text>',</text>
                        <text>y: </text>
                        @item.Value
                        <text>},</text>
                    }
                ]
            }
        ]
    });
</script>

<script>
    $('#AppealByStatus').highcharts({
        chart: {
            plotBackgroundColor: null,
            plotBorderWidth: null,
            plotShadow: false,
            type: 'pie'
        },
        title: {
            text: 'Appeal By Status'
        },
        tooltip: {
            pointFormat: '{series.name}: {point.y} appeal(s) <b>{point.percentage:.1f}%</b>'
        },
        plotOptions: {
            pie: {
                allowPointSelect: true,
                cursor: 'pointer',
                dataLabels: {
                    enabled: false
                },
                showInLegend: true
            }
        },
        series: [
            {
                name: 'Appeal',
                colorByPoint: true,
                data: [
                    @foreach (var item in Model.AppealByStatus)
                    {
                        <text>{name:'</text>@item.Key<text>',</text>
                        <text>y: </text>
                        @item.Value
                        <text>},</text>
                    }
                ]
            }
        ]
    });
</script>
