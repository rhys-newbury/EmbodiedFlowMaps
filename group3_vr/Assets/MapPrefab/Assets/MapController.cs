using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

/// <summary>
/// A controlling instance for a series of MapContainers.
/// </summary>
public class MapController : MonoBehaviour
{ 
    public string mainMap;
    public string pathToStates;
    public string pathToData;

    public Legend CountryLegend;
    //public ThicknessLegend FlowLegend;

    public Stack StateStack;

    public Stack CountyStack;


    public static Color empty_colour = new Color(0,0,0,0.2F);


    private Dictionary<string, Dictionary<string, float>> flow = new Dictionary<string, Dictionary<string, float>>();
    //State -> County -> State -> County
    private Dictionary<string, Dictionary<string, Dictionary<string, Dictionary<string, float>>>> county_flow = new Dictionary<string, Dictionary<string, Dictionary<string, Dictionary<string, float>>>>();


    private Dictionary<string, Dictionary<string, float>> populationDenisty = new Dictionary<string, Dictionary<string, float>>();
    
    private Dictionary<string, Dictionary<string, float>> countyIncoming = new Dictionary<string, Dictionary<string, float>>();

    private Dictionary<string, Dictionary<string, float>> countyOutcoming = new Dictionary<string, Dictionary<string, float>>();



  

    public static bool isIncoming = true;

    internal bool startUp = true;

    public bool haveTooltip;

    public Vector3 mapScale;


    /// <summary>
    /// Load dataset on startup
    /// </summary>
    /// <returns></returns>
    /// 
    void Start()
    {

        populationDenisty.Add("America", new Dictionary<string, float>());

        StreamReader pop_dens_stream = new StreamReader(pathToData + "state_population_denisty.csv");

        while (!pop_dens_stream.EndOfStream)
        {
            string inp_ln = pop_dens_stream.ReadLine();

            string[] data = inp_ln.Split(',');
            string state = data[0];
            float inc_data = float.Parse(data[1]);

            populationDenisty["America"].Add(state, inc_data);


        }

        StreamReader cpop_dens_stream = new StreamReader(pathToData + "county_population_density.csv", System.Text.Encoding.GetEncoding("iso-8859-1"));

        while (!cpop_dens_stream.EndOfStream)
        {
            string inp_ln = cpop_dens_stream.ReadLine();

            string[] data = inp_ln.Split(',');
            string state = data[1].Trim();
            string county = data[0].Trim();


            float inc_data = float.Parse(data[2]);

            if (!populationDenisty.ContainsKey(state))
            {
                populationDenisty.Add(state, new Dictionary<string, float>());
            }
            populationDenisty[state].Add(county, inc_data);
        }





        StreamReader inp_stm = new StreamReader(pathToData+"inc.csv");

        countyIncoming.Add("America", new Dictionary<string, float>());



        while (!inp_stm.EndOfStream)
        {
            string inp_ln = inp_stm.ReadLine();

            string[] data = inp_ln.Split(',');
            string state = data[0];
            float inc_data = float.Parse(data[1]);

            countyIncoming["America"].Add(state, inc_data);


        }
        StreamReader cinp_stm = new StreamReader(pathToData + "county_inc.txt", System.Text.Encoding.GetEncoding("iso-8859-1"));

        while (!cinp_stm.EndOfStream)
        {
            string inp_ln = cinp_stm.ReadLine();

            string[] data = inp_ln.Split(',');
            string state = data[0].Trim();
            string county = data[1].Trim();


            float inc_data = float.Parse(data[2]);

            if (!countyIncoming.ContainsKey(state))
            {
                countyIncoming.Add(state, new Dictionary<string, float>());
            }

            countyIncoming[state].Add(county, inc_data);
        }


        StreamReader out_stm = new StreamReader(pathToData + "out.csv");

        countyOutcoming.Add("America", new Dictionary<string, float>());

        while (!out_stm.EndOfStream)
        {
            string out_ln = out_stm.ReadLine();

            string[] data = out_ln.Split(',');
            string state = data[0];
            float out_data = float.Parse(data[1]);

            countyOutcoming["America"].Add(state, out_data);


        }
        StreamReader cout_stm = new StreamReader(pathToData + "county_out.txt", System.Text.Encoding.GetEncoding("iso-8859-1"));

        while (!cout_stm.EndOfStream)
        {
            string cout_ln = cout_stm.ReadLine();

            string[] data = cout_ln.Split(',');
            string state = data[0].Trim();
            string county = data[1].Trim();


            float cout_data = float.Parse(data[2]);

            if (!countyOutcoming.ContainsKey(state))
            {
                countyOutcoming.Add(state, new Dictionary<string, float>());
            }

            countyOutcoming[state].Add(county, cout_data);
        }





        StreamReader inp_stm2 = new StreamReader(pathToData + "flow.csv", System.Text.Encoding.GetEncoding("iso-8859-1"));

        county_flow["America"] = new Dictionary<string, Dictionary<string, Dictionary<string, float>>>();

        while (!inp_stm2.EndOfStream)
        {
            string inp_ln = inp_stm2.ReadLine();



            string[] data = inp_ln.Split(',');
            string state1 = data[0].Trim();
            string state2 = data[1].Trim();
            float inc_data = float.Parse(data[2]);

            

            if (!county_flow["America"].ContainsKey(state1))
            {
                county_flow["America"].Add(state1, new Dictionary<string, Dictionary<string, float>>());
            }

            if (!county_flow["America"][state1].ContainsKey("America"))
            {
                county_flow["America"][state1].Add("America",new Dictionary<string, float>());

            }

            county_flow["America"][state1]["America"][state2] = inc_data;

        }

        StreamReader inp_stm3 = new StreamReader(pathToData + "county_flow.txt", System.Text.Encoding.GetEncoding("iso-8859-1"));
        
        while (!inp_stm3.EndOfStream)
        {
            string inp_ln = inp_stm3.ReadLine();



            string[] data = inp_ln.Split(',');
            string county1 = data[1].Trim();
            string state1 = data[0].Trim();
            string county2 = data[3].Trim();
            string state2 = data[2].Trim();

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


            if (!county_flow.ContainsKey("America"))
            {
                county_flow["America"] = new Dictionary<string, Dictionary<string, Dictionary<string, float>>>();
            }

            if (!county_flow["America"].ContainsKey(state2))
            {
                county_flow["America"][state2] = new Dictionary<string, Dictionary<string, float>>();
            }

            if (!county_flow["America"][state2].ContainsKey(state1))
            {
                county_flow["America"][state2][state1] = new Dictionary<string, float>();
            }

            if (!county_flow["America"][state2][state1].ContainsKey(county1))
            {
                county_flow["America"][state2][state1][county1] = 0;
            }
                                          


            county_flow["America"][state2][state1][county1] += inc_data;


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


    


}


    public float getPopulationDensity(string current, string parent)
    {
        if (populationDenisty.ContainsKey(parent))
        {
            if (populationDenisty[parent].ContainsKey(current))
            {
                return populationDenisty[parent][current];
            }
            else
            {
                return -1;

            }
        }

        else
        {
            return -1;
        }
    }


    /// <summary>
    /// Get the data from dataset.
    /// </summary>
    /// <returns>return amount of data, -1 for no data</returns>
    /// 
    public float getData(string current, string parent, bool incoming)
    {
        if (incoming)
        {
            if (countyIncoming.ContainsKey(parent) && countyIncoming[parent].ContainsKey(current))
            {
                return countyIncoming[parent][current];
            }
            else
            {
                return -1;
            }
        }
        else
        {
            if (countyOutcoming.ContainsKey(parent) && countyOutcoming[parent].ContainsKey(current))
            {
                return countyOutcoming[parent][current];
            }
            else
            {
                return -1;
            }

        }
    }
    /// <summary>
    /// Get flow between two current objects with parents.
    /// </summary>
    /// <returns>float representing amount of flow</returns>
    /// 
    public float getFlowData(string current1, string parent1, string current2, string parent2)
    {
        try
        {
            return county_flow[parent1][current1][parent2][current2];

        }
        catch
        {
            return -1;
        }
    }

       

    /// <summary>
    /// Get colour based on legend seperators.
    /// </summary>
    /// <returns>Color representing the data</returns>
    /// 
    internal Color getCountryColour(float data)
    {

        if (data == -1)
        {
            return empty_colour;
        }
        else if (data > 0 && data < CountryLegend.ClassSeperator1)
        {
            return CountryLegend.scheme[0];
        }
        else if (data >= CountryLegend.ClassSeperator1 && data < CountryLegend.ClassSeperator2)
        {
            return CountryLegend.scheme[1];
        }
        else if (data >= CountryLegend.ClassSeperator2 && data < CountryLegend.ClassSeperator3)
        {
            return CountryLegend.scheme[2];
        }

        else if (data >= CountryLegend.ClassSeperator3 && data < CountryLegend.ClassSeperator4)
        {
            return CountryLegend.scheme[3];
        }
        else if (data >= CountryLegend.ClassSeperator4 && data < CountryLegend.ClassSeperator5)
        {
            return CountryLegend.scheme[4];
        }
        else if (data >= CountryLegend.ClassSeperator5 && data < CountryLegend.ClassSeperator6)
        {
            return CountryLegend.scheme[5];
        }
        else
        {
            return CountryLegend.scheme[6];
        }
    }

    //internal float getFlowColour(float data)
    //{
    //    var material = Resources.Load("Materials/GlowingBlue1", typeof(Material)) as Material;
    //    if (data == -1)
    //    {
    //        return 0;
    //    }
    //    if (data > 0 && data < FlowLegend.ClassSeperator1)
    //    {
    //        return 0.02F;
    //    }
    //    else if (data > FlowLegend.ClassSeperator1 && data < FlowLegend.ClassSeperator2)
    //    {
    //        return 0.03F;
    //    }
    //    else if (data > FlowLegend.ClassSeperator2 && data < FlowLegend.ClassSeperator3)
    //    {
    //        return 0.04F;
    //    }

    //    else if (data > FlowLegend.ClassSeperator3 && data < FlowLegend.ClassSeperator4)
    //    {
    //        return 0.05F;

    //    }
    //    else if (data > FlowLegend.ClassSeperator4 && data < FlowLegend.ClassSeperator5)
    //    {
    //        return 0.06F;
    //    }
    //    else if (data > FlowLegend.ClassSeperator5 && data < FlowLegend.ClassSeperator6)
    //    {
    //        return 0.07F;
    //    }
    //    else
    //    {
    //        return 0.08F;

    //    }
    //}


    // public Dictionary<string, HashSet<string>> list_of_counties = new Dictionary<string, HashSet<string>>();

    /// <summary>
    /// Convert county code to State Name
    /// </summary>
    /// <returns>State name as a string</returns>
    /// 
    private static string codeToState(string code)
    {
        switch (code)
        {
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
