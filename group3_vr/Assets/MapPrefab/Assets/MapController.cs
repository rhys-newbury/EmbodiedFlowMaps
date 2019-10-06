using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

public class MapController : MonoBehaviour
{ 
    public string mainMap;
    public string pathToStates;
    public string pathToData;

    public Legend CountyLegend;
    public Legend FlowLegend;

    public static Color empty_colour = new Color(0,0,0,0.2F);


    private Dictionary<string, Dictionary<string, float>> flow = new Dictionary<string, Dictionary<string, float>>();
    //State -> County -> State -> County
    private Dictionary<string, Dictionary<string, Dictionary<string, Dictionary<string, float>>>> county_flow = new Dictionary<string, Dictionary<string, Dictionary<string, Dictionary<string, float>>>>();



    private Dictionary<string, Dictionary<string, float>> countyIncoming = new Dictionary<string, Dictionary<string, float>>();

    private Dictionary<string, Dictionary<string, float>> countyOutcoming = new Dictionary<string, Dictionary<string, float>>();



    //List of string data for the buildings. This can be converted in to actual buildings on county creation
    private Dictionary<String, Dictionary<String, List<List<String>>>> _buildingData = null;

    //List of buildings
    public Dictionary<String, Dictionary<String, List<Buildings>>> buildingData = new Dictionary<String, Dictionary<String, List<Buildings>>>();

    public static bool isIncoming = true;



    public bool checkForBuildings(string County, string State)
    {
        return _buildingData.ContainsKey(State) && _buildingData[State].ContainsKey(County);
    }

    public bool startUp = true;

    public bool haveTooltip;

    public Vector3 mapScale;



    void Start()
    {



        StreamReader inp_stm = new StreamReader(pathToData+"inc.csv");

        countyIncoming.Add("America", new Dictionary<string, float>());



        while (!inp_stm.EndOfStream)
        {
            string inp_ln = inp_stm.ReadLine();

            string[] data = inp_ln.Split(',');
            string state = codeToState(data[0]);
            float inc_data = float.Parse(data[1]);

            countyIncoming["America"].Add(state, inc_data);


        }
        StreamReader cinp_stm = new StreamReader(pathToData + "county_in.csv");

        while (!cinp_stm.EndOfStream)
        {
            string inp_ln = cinp_stm.ReadLine();

            string[] data = inp_ln.Split(',');
            string state = data[1];
            string county = data[0];


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
            string state = codeToState(data[0]);
            float out_data = float.Parse(data[1]);

            countyOutcoming["America"].Add(state, out_data);


        }
        StreamReader cout_stm = new StreamReader(pathToData + "county_out.csv");

        while (!cout_stm.EndOfStream)
        {
            string cout_ln = cout_stm.ReadLine();

            string[] data = cout_ln.Split(',');
            string state = data[1];
            string county = data[0];


            float cout_data = float.Parse(data[2]);

            if (!countyOutcoming.ContainsKey(state))
            {
                countyOutcoming.Add(state, new Dictionary<string, float>());
            }

            countyOutcoming[state].Add(county, cout_data);
        }





        StreamReader inp_stm2 = new StreamReader(pathToData + "flow.csv");

        county_flow["America"] = new Dictionary<string, Dictionary<string, Dictionary<string, float>>>();

        while (!inp_stm2.EndOfStream)
        {
            string inp_ln = inp_stm2.ReadLine();



            string[] data = inp_ln.Split(',');
            string state1 = codeToState(data[0]);
            string state2 = codeToState(data[1]);
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

        StreamReader inp_stm3 = new StreamReader(pathToData + "county_flow.csv");
        
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


        //Create a dictionary.
        _buildingData = new Dictionary<String, Dictionary<String, List<List<String>>>>();
        StreamReader inp_stm5 = new StreamReader(pathToData + "building_data.csv");



        while (!inp_stm5.EndOfStream)
        {

            string inp_ln = inp_stm5.ReadLine();

            List<String> data = inp_ln.Split(',').ToList();

            //Check for existance otherwise add a new dictionary
            if (!_buildingData.ContainsKey(data[4]))
            {
                _buildingData.Add(data[4], new Dictionary<string, List<List<string>>>());
            }
            Dictionary<String, List<List<String>>> stateData = _buildingData[data[4]];


            if (!stateData.ContainsKey(data[3]))
            {
                stateData.Add(data[3], new List<List<string>>());
            }

            //Add the data to the state
            stateData[data[3]].Add(data);

        }
    


}

    internal void addToList(string parentName, string name)
    {
        if (!list_of_counties.ContainsKey(parentName))
        {
            list_of_counties[parentName] = new HashSet<string>();
        }

        list_of_counties[parentName].Add(name);
    }

    internal Dictionary<String, Dictionary<String, List<List<String>>>> getBuildingData()
    {
        return _buildingData;
    }

    public float getData(string current, string parent)
    {
        if (isIncoming)
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

    public float getFlowData(string current1, string parent1, string current2, string parent2)
    {
        try
        {
            return Mathf.Max(county_flow[parent1][current1][parent2][current2], county_flow[parent2][current2][parent1][current1]);
        }
        catch
        {
            try
            {
                return county_flow[parent2][current2][parent1][current1];
            }

            catch
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

            }
  
        }
    

    internal Color getCountryColour(float data)
    {

        if (data == -1)
        {
            return empty_colour;
        }
        else if (data > 0 && data < CountyLegend.ClassSeperator1)
        {
            return CountyLegend.Colour1;
        }
        else if (data > CountyLegend.ClassSeperator1 && data < CountyLegend.ClassSeperator2)
        {
            return CountyLegend.Colour2;
        }
        else if (data > CountyLegend.ClassSeperator2 && data < CountyLegend.ClassSeperator3)
        {
            return CountyLegend.Colour3;
        }

        else if (data > CountyLegend.ClassSeperator3 && data < CountyLegend.ClassSeperator4)
        {
            return CountyLegend.Colour4;
        }
        else if (data > CountyLegend.ClassSeperator4 && data < CountyLegend.ClassSeperator5)
        {
            return CountyLegend.Colour5;
        }
        else if (data > CountyLegend.ClassSeperator5 && data < CountyLegend.ClassSeperator6)
        {
            return CountyLegend.Colour6;
        }
        else
        {
            return CountyLegend.Colour7;
        }
    }

    internal Material getFlowColour(float data)
    {

        if (data > 0 && data < FlowLegend.ClassSeperator1)
        {
            return Resources.Load("Materials/GlowingBlue1", typeof(Material)) as Material;
        }
        else if (data > FlowLegend.ClassSeperator1 && data < FlowLegend.ClassSeperator2)
        {
            return Resources.Load("Materials/GlowingBlue2", typeof(Material)) as Material;
        }
        else if (data > FlowLegend.ClassSeperator2 && data < FlowLegend.ClassSeperator3)
        {
            return Resources.Load("Materials/GlowingBlue3", typeof(Material)) as Material;
        }

        else if (data > FlowLegend.ClassSeperator3 && data < FlowLegend.ClassSeperator4)
        {
            return Resources.Load("Materials/GlowingBlue4", typeof(Material)) as Material;
        }
        else if (data > FlowLegend.ClassSeperator4 && data < FlowLegend.ClassSeperator5)
        {
            return Resources.Load("Materials/GlowingBlue5", typeof(Material)) as Material;
        }
        else if (data > FlowLegend.ClassSeperator5 && data < FlowLegend.ClassSeperator6)
        {
            return Resources.Load("Materials/GlowingBlue6", typeof(Material)) as Material;
        }
        else
        {
            return Resources.Load("Materials/GlowingBlue7", typeof(Material)) as Material;
        }
    }


    public Dictionary<string, HashSet<string>> list_of_counties = new Dictionary<string, HashSet<string>>();


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


   

    // Update is called once per frame
    void Update()
    {
        
    }
}
