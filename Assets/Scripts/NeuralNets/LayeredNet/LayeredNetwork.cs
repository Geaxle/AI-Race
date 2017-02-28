﻿using UnityEngine;
using nfs.tools;

namespace nfs.layered {

    ///<summary>
    /// Neural network class. This is a fully connected deep layered network
    /// It can have varying number of neurons and layers.
    /// The network always has a single bias input neuron.
    ///</summary>
    public class LayeredNetwork {

        public float FitnessScore { set; get; }

        // layer properties
        private Matrix inputNeurons;
        private Matrix[] hiddenLayersNeurons;
        private Matrix outputNeurons;
        private Matrix[] synapses;

        ///<summary>
        /// Layered neural network constructor.
        /// Requires a given number of input, number given of output
        /// and an array for the hidden layers with each element being the size of a different hidden layer.!--
        ///</summary>
        public LayeredNetwork(int inputLayerSize, int outputLayerSize, int[] hiddenLayersSizes) {
            // each layer is one line of neuron
            inputNeurons = new Matrix(1, inputLayerSize).SetToOne();
            outputNeurons = new Matrix(1, outputLayerSize).SetToOne();

            // hidden layer is an array of matrix of one line
            hiddenLayersNeurons = new Matrix[hiddenLayersSizes.Length];
            for (int i = 0; i < hiddenLayersSizes.Length; i++) {
                hiddenLayersNeurons[i] = new Matrix(1, hiddenLayersSizes[i]).SetToOne();
            }

            // synapses are an array of matrix
            // the number of line (or array of synapses sort of) is equal to the previous layer (neurons coming from)
            // the number of column (or nb of synapses in a row) is equal to the next layer (neurons going to)
            synapses = new Matrix[hiddenLayersSizes.Length + 1];
            for (int i = 0; i < synapses.Length; i++) {
                if (i == 0) // input synapses
                    synapses[i] = new Matrix(inputLayerSize, hiddenLayersSizes[i]).SetAsSynapse();
                else if (i == synapses.Length - 1) // synapses to output
                    synapses[i] = new Matrix(hiddenLayersSizes[i - 1], outputLayerSize).SetAsSynapse();
                else // middle synapses
                    synapses[i] = new Matrix(hiddenLayersSizes[i - 1], hiddenLayersSizes[i]).SetAsSynapse();
            }
        }

        ///<summary>
        /// Creates and return a deep clone of the network.
        ///</summary>
        public LayeredNetwork GetClone () {
            int hiddenLayers = this.hiddenLayersNeurons.Length;
            int[] hiddenLayerSizes = new int[hiddenLayers];
            for(int i=0; i<hiddenLayers; i++) {
                hiddenLayerSizes[i] = this.hiddenLayersNeurons[i].J;
            }

            LayeredNetwork clone = new LayeredNetwork(this.inputNeurons.J, this.outputNeurons.J, hiddenLayerSizes);

            clone.InsertSynapses(this.GetSynapsesClone());
            clone.FitnessScore = FitnessScore;

            return clone;
        }


        // this is to pass the activation function on the neuron value and on each layer
        // a layer cannot have more than one line so we don't loop through the I
        private void ProcessActivation (Matrix mat) {
            for(int j=0; j < mat.J; j++) {
                //mat.Mtx[0][j] = Sigmoid(mat.Mtx[0][j]);
                //mat.Mtx[0][j] = Linear(mat.Mtx[0][j]);
                mat.Mtx[0][j] = TanH(mat.Mtx[0][j]);
            }
        }

        private float TanH (float t) {
            return (2f / (1f + Mathf.Exp(-2f*t))) - 1f;
        }

        private float Sigmoid(float t) {
            return 1f / (1f + Mathf.Exp(-t));
        }

        private float Linear(float t) {
            return t;
        }

        ///<summary>
        /// Process the inputs forward to get outputs in the network.
        ///</summary>
		public float[] PingFwd(float[] sensorsValues) {
            
            // we set the inputs neurons values and ignore the missmatch as there is a bias neuron
            inputNeurons.SetLineValues(0, sensorsValues, true); 

            // we ping the network
            for (int i = 0; i < hiddenLayersNeurons.Length + 1; i++) {
                    if (i == 0) {
                        hiddenLayersNeurons[0] = Matrix.Multiply(inputNeurons, synapses[0]);
                        ProcessActivation(hiddenLayersNeurons[0]);
                    } else if (i == hiddenLayersNeurons.Length) {
                        outputNeurons = Matrix.Multiply(hiddenLayersNeurons[i - 1], synapses[i]);
                        ProcessActivation(outputNeurons);
                        
                    } else {
                        hiddenLayersNeurons[i] = Matrix.Multiply(hiddenLayersNeurons[i - 1], synapses[i]);
                        ProcessActivation(hiddenLayersNeurons[i]);
                    }
                }

            return outputNeurons.GetLineValues();
        }

        ///<summary>
        /// Get the number of input neurons.
        ///</summary>
        public int GetInputSize () {
            return this.inputNeurons.J;
        }

        ///<summary>
        /// Get the number of outpu neurons.
        ///</summary>
        public int GetOutputSize () {
            return this.outputNeurons.J;
        }

        ///<summary>
        /// Get the number of neurons in each hidden layers.
        ///</summary>
        public int[] GetHiddenLayersSizes() {
            int[] hiddenLayerSizes = new int[hiddenLayersNeurons.Length];
            for (int i = 0; i < hiddenLayersNeurons.Length; i++) {
                hiddenLayerSizes[i] = hiddenLayersNeurons[i].J;
            }

            return hiddenLayerSizes;
        }

        ///<summary>
        /// Get all output values.
        ///</summary>
        public float[] GetOutputValues() {
            return this.outputNeurons.GetLineValues(0);
        }

        ///<summary>
        /// Get a deep clone of all the synapses as an array of Matrices.
        ///</summary>
        public Matrix[] GetSynapsesClone () {
            Matrix[] synapsesCopy = new Matrix[synapses.Length];
            for (int i = 0; i < synapses.Length; i++) {
                synapsesCopy[i] = synapses[i].GetClone();
            }
            return synapsesCopy;
        }

        ///<summary>
        /// Replace the synapse values with new ones from an array of Matrices.
        ///</summary>
        public void InsertSynapses(Matrix[] newSynapses) {

            if(synapses.Length == newSynapses.Length){
                for (int i = 0; i < synapses.Length; i++) {
                    synapses[i].SetAllValues(newSynapses[i]);
                }  
            } else {
                Debug.LogWarning("The number of synapses matrices to insert does not match the number of this network: "
                                + newSynapses.Length + " vs " + synapses.Length  + ", doing nothing.");
            }
        }
    }
}
