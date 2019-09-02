using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public static class dataAccessor
{
    private static Dictionary<string, float> stateIncoming = new Dictionary<string, float>();
   
    public static void load()
    {

        StreamReader inp_stm = new StreamReader("C:\\Users\\FIT3161\\Desktop\\group3\\group3_vr\\data_processing_scripts\\inc.csv");

        while (!inp_stm.EndOfStream)
        {
            string inp_ln = inp_stm.ReadLine();



            string[] data = inp_ln.Split(',');
            string state = codeToState(data[0]);
            float inc_data = float.Parse(data[1]);

            stateIncoming.Add(state, inc_data);
                    
        }



    }

    public static float getData(string State)
    {
        if (stateIncoming.ContainsKey(State))
        {
            return stateIncoming[State];
        } else
        {
            return Random.Range(0, 1000000);
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
