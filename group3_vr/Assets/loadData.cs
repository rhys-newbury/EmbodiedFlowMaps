using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public static class dataAccessor
{
    private static Dictionary<string, Dictionary<string, float>> flow = new Dictionary<string, Dictionary<string, float>>();
    //State -> County -> State -> County
    private static Dictionary<string, Dictionary<string, Dictionary<string, Dictionary<string, float>>>> county_flow = new Dictionary<string, Dictionary<string, Dictionary<string, Dictionary<string, float>>>>();
    

    private static Dictionary<string, float> stateIncoming = new Dictionary<string, float>();

    public static void load()
    {

        StreamReader inp_stm = new StreamReader("D:\\vr\\group3_vr\\data_processing_scripts\\inc.csv");

        while (!inp_stm.EndOfStream)
        {
            string inp_ln = inp_stm.ReadLine();



            string[] data = inp_ln.Split(',');
            string state = codeToState(data[0]);
            float inc_data = float.Parse(data[1]);

            stateIncoming.Add(state, inc_data);



        }

        StreamReader inp_stm2 = new StreamReader("D:\\vr\\group3_vr\\data_processing_scripts\\flow.csv");

        while (!inp_stm2.EndOfStream)
        {
            string inp_ln = inp_stm2.ReadLine();



            string[] data = inp_ln.Split(',');
            string state1 = codeToState(data[0]);
            string state2 = codeToState(data[1]);
            float inc_data = float.Parse(data[2]);

            if (!flow.ContainsKey(state1))
            {
                flow[state1] = new Dictionary<string, float>();
            }

            flow[state1][state2] = inc_data;

        }


        StreamReader inp_stm3 = new StreamReader("D:\\vr\\group3_vr\\data_processing_scripts\\county_flow.csv");

        while (!inp_stm3.EndOfStream)
        {
            string inp_ln = inp_stm3.ReadLine();



            string[] data = inp_ln.Split(',');
            string county1 = data[0];
            string state1 = data[1];
            string county2 = data[2];
            string state2 = data[3];

            float inc_data = float.Parse(data[4]);

            if (!county_flow.ContainsKey(state1))
            {
                county_flow[state1] = new Dictionary<string, Dictionary<string, Dictionary<string, float>>>();
            }

            var current_state = county_flow[state1];

            if (!current_state.ContainsKey(county1))
            {
                current_state[county1] = new Dictionary<string, Dictionary<string, float>>();
            }

            var current_county = current_state[county1];

            if (!current_county.ContainsKey(state2))
            {
                current_county[state2] = new Dictionary<string, float>();
            }

             current_county[state2][county2] = inc_data;


            if (!current_county.ContainsKey("America"))
            {
                current_county["America"] = new Dictionary<string, float>();
            }

            if (!current_county["America"].ContainsKey(state2))
            {
                current_county["America"][state2] = 0;
            }

            current_county["America"][state2] += inc_data;

        }

        Debug.Log(county_flow);

    }

    internal static void addToList(string parentName, string name)
    {
        if (!list_of_counties.ContainsKey(parentName))
        {
            list_of_counties[parentName] = new HashSet<string>();
        }

        list_of_counties[parentName].Add(name);
    }

    public static float getData(string State)
    {
        if (stateIncoming.ContainsKey(State))
        {
            return stateIncoming[State];
        } else
        {
            return UnityEngine.Random.Range(0, 1000000);
        }
    }

    public static Color getColour(float data)
    {
        Color out_;
        if (data > 0 && data < 85000)
        {
            ColorUtility.TryParseHtmlString("#fee5d9", out out_);
        }
        else if (data > 85000 && data < 280000)
        {
            ColorUtility.TryParseHtmlString("#fcae91", out out_);
        }
        else if (data > 280000 && data < 600000)
        {
            ColorUtility.TryParseHtmlString("#fb6a4a", out out_);
        }
        else
        {
            ColorUtility.TryParseHtmlString("#cb181d", out out_);
        }
        return out_;
    }

    //public static string[] list_of_states = (new string[] { "Alabama", "Arizona", "Arkansas", "California", "Colorado", "Connecticut", "Delaware", "District of Columbia", "Florida", "Georgia", "Idaho", "Illinois", "Indiana", "Iowa", "Kansas", "Kentucky", "Louisiana", "Maine", "Maryland", "Massachusetts", "Michigan", "Minnesota","Mississippi", "Missouri", "Montana", "Nebraska", "Nevada", "New Hampshire", "New Jersey", "New Mexico", "New York", "North Carolina", "North Dakota", "Ohio", "Oklahoma", "Oregon", "Pennsylvania", "Rhode Island", "South Carolina", "South Dakota", "Tennessee", "Texas", "U.S. Virgin Islands", "Utah", "Vermont", "Virginia", "Washington", "West Virginia", "Wisconsin", "Wyoming" });

    public static Dictionary<string, HashSet<string>> list_of_counties = new Dictionary<string, HashSet<string>>();


    private static string codeToState(string code)
    {
        switch (code) { 
            case "AL":
                return "Alabama";
            case "AK":
                return "Alaska";
            case "AZ":
                return "Arizona";
            case "AR":
                return "Arkansas";
            case "CA":
                return "California";
            case "CO":
                return "Colorado";
            case "CT":
                return "Connecticut";
            case "DE":
                return "Delaware";
            case "DC":
                return "District of Columbia";
            case "FL":
                return "Florida";
            case "GA":
                return "Georgia";
            case "HI":
                return "Hawaii";
            case "ID":
                return "Idaho";
            case "IL":
                return "Illinois";
            case "IN":
                return "Indiana";
            case "IA":
                return "Iowa";
            case "KS":
                return "Kansas";
            case "KY":
                return "Kentucky";
            case "LA":
                return "Louisiana";
            case "ME":
                return "Maine";
            case "MD":
                return "Maryland";
            case "MA":
                return "Massachusetts";
            case "MI":
                return "Michigan";
            case "MN":
                return "Minnesota";
            case "MS":
                return "Mississippi";
            case "MO":
                return "Missouri";
            case "MT":
                return "Montana";
            case "NE":
                return "Nebraska";
            case "NV":
                return "Nevada";
            case "NH":
                return "New Hampshire";
            case "NJ":
                return "New Jersey";
            case "NM":
                return "New Mexico";
            case "NY":
                return "New York";
            case "NC":
                return "North Carolina";
            case "ND":
                return "North Dakota";
            case "OH":
                return "Ohio";
            case "OK":
                return "Oklahoma";
            case "OR":
                return "Oregon";
            case "PA":
                return "Pennsylvania";
            case "RI":
                return "Rhode Island";
            case "SC":
                return "South Carolina";
            case "SD":
                return "South Dakota";
            case "TN":
                return "Tennessee";
            case "TX":  
                return "Texas";
            case "UT":
                return "Utah";
            case "VT":
                return "Vermont";
            case "VA":
                return "Virginia";
            case "WA":
                return "Washington";
            case "WV":
                return "West Virginia";
            case "WI":
                return "Wisconsin";
            case "WY":
                return "Wyoming";
            default:
                return "Texas";
    }
}
}
