﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.DesignScript.Runtime;
using Dynamo.Controls;
using Dynamo.Graph.Nodes;
using Dynamo.Wpf;
using ProtoCore.AST.AssociativeAST;
using Newtonsoft.Json;
using DynamoPandas.Pandamo.Pandas;
using DynamoPandas.Pandamo.Format;
using DynamoPandas.PandamoNodeModels.Controls;
using System.Collections;
using DynamoPandas.Pandamo.Utilities;

namespace DynamoPandas.PandamoNodeModels.Nodes
{
    [NodeName("Tabulate")]
    [NodeCategory("Pandamo.Format")]
    [NodeDescription("")]
    // The InPortNames attribute determines the
    // amount of input ports of your node and their names.
    [InPortNames("Dataframe")]
    [InPortTypes("DataFrame")]
    // The InPortDescriptions attribute sets the description
    // of your input ports in the tooltip shown when you hover them.
    [InPortDescriptions("DataFrame")]

    // The OutPortNames attribute determines the
    // amount of output ports of your node and their names.
    [OutPortNames("string")]
    [OutPortTypes("string")]
    [IsDesignScriptCompatible]
    public class DataframeTableNodeModel : NodeModel
    {
        public Dictionary<string,object> DataframeDictionary { get; set; }
        
        #region Constructors
        public DataframeTableNodeModel()
        {
            RegisterAllPorts();
            PortDisconnected += DataframeFormatNodeModel_PortDisconnected;
            ArgumentLacing = LacingStrategy.Disabled;
        }

        [JsonConstructor]
        public DataframeTableNodeModel(IEnumerable<PortModel> inPorts, IEnumerable<PortModel> outPorts) : base(inPorts, outPorts)
        {
            PortDisconnected += DataframeFormatNodeModel_PortDisconnected;
        }
        #endregion
        
        #region Events
        private void DataframeFormatNodeModel_PortDisconnected(PortModel port)
        {
            if (DataframeDictionary != null)
            {
                DataframeDictionary.Clear();
                RaisePropertyChanged("DataUpdated");
            }
        }
        #endregion
        
        #region Databridge
        /// <summary>
        /// Register the data bridge callback.
        /// </summary>
        protected override void OnBuilt()
        {
            base.OnBuilt();
            VMDataBridge.DataBridge.Instance.RegisterCallback(GUID.ToString(), DataBridgeCallback);
        }

        /// <summary>
        /// Callback method for DataBridge mechanism.
        /// This callback only gets called when 
        ///     - The AST is executed
        ///     - After the BuildOutputAST function is executed 
        ///     - The AST is fully built
        /// </summary>
        /// <param name="data">The data passed through the data bridge.</param>
        private void DataBridgeCallback(object data)
        {
            // Grab input data which always returned as an ArrayList
            var inputs = data as ArrayList;

            // Each of the list inputs are also returned as ArrayLists
            Dictionary<string, object> dataframeDictionary = DictionaryHelpers.ToCDictionary(inputs[0] as DesignScript.Builtin.Dictionary);
      
            DataframeDictionary = dataframeDictionary;
            // Notify UI the data has been modified
            RaisePropertyChanged("DataUpdated");
        }
        #endregion

        #region Ast
        /// <summary>
        /// BuildOutputAst is where the outputs of this node are calculated.
        /// This method is used to do the work that a compiler usually does 
        /// by parsing the inputs List inputAstNodes into an abstract syntax tree.
        /// </summary>
        /// <param name="inputAstNodes"></param>
        /// <returns></returns>
        [IsVisibleInDynamoLibrary(false)]
        public override IEnumerable<AssociativeNode> BuildOutputAst(List<AssociativeNode> inputAstNodes)
        {
            // WARNING!!!
            // Do not throw an exception during AST creation.

            // If inputs are not connected return null
            if (!InPorts[0].IsConnected)
            {
                return new[]
                {
                    AstFactory.BuildAssignment(GetAstIdentifierForOutputIndex(0), AstFactory.BuildNullNode()),
                };
            }

            // Build a function call to a C# function which lives in a different DLL assembly
            // and use input nodes 0 and 1 as input values in the fuction
            // Note that we specify input and output value types in new Func<double, double, double>
            // which means we have two double inputs and one double output
            AssociativeNode inputNode = AstFactory.BuildFunctionCall(
                new Func<DataFrame, object>(DataFrame.ToInternalDictionary),
                new List<AssociativeNode> { inputAstNodes[0]}
            );

            return new[]
            {
                AstFactory.BuildAssignment(GetAstIdentifierForOutputIndex(0), inputNode),
                AstFactory.BuildAssignment(
                        AstFactory.BuildIdentifier(AstIdentifierBase + "_dummy"),
                        VMDataBridge.DataBridge.GenerateBridgeDataAst(GUID.ToString(), AstFactory.BuildExprList(
                            new List<AssociativeNode>{ inputNode })
                    )
                )
            };
        }
        #endregion
    }

    /// <summary>
    ///     View customizer for CustomNodeModel Node Model.
    /// </summary>
    public class DataframeFormatNodeView : INodeViewCustomization<DataframeTableNodeModel>
    {
        private DataframeFormatControl dataframeFormatControl;

        /// <summary>
        /// At run-time, this method is called during the node 
        /// creation. Add custom UI element to the node view.
        /// </summary>
        /// <param name="model">The NodeModel representing the node's core logic.</param>
        /// <param name="nodeView">The NodeView representing the node in the graph.</param>
        public void CustomizeView(DataframeTableNodeModel model, NodeView nodeView)
        {
            dataframeFormatControl = new DataframeFormatControl(model);
            nodeView.inputGrid.Children.Add(dataframeFormatControl);
        }

        /// <summary>
        /// Here you can do any cleanup you require if you've assigned callbacks for particular 
        /// UI events on your node.
        /// </summary>
        public void Dispose() { }
    }
}
