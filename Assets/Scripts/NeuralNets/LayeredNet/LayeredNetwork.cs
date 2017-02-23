﻿using UnityEngine;
using nfs.tools;

namespace nfs.layered {
    public class LayeredNetwork {

        private Matrix inputNeurons;
        private Matrix[] hiddenLayersNeurons;
        private Matrix outputNeurons;
        private Matrix[] synapses;

        public LayeredNetwork(int inputLayerSize, int outputLayerSize, int[] hiddenLayersSizes) {
            // each layer is one line of neuron

            // input layer is a matrix of 1 line only 
            inputNeurons = new Matrix(1, inputLayerSize).SetToOne();  // default should be 3

            // output layer is a matrix of 1 line only
            outputNeurons = new Matrix(1, outputLayerSize).SetToOne(); // default should be 2

            // hidden layer is an array of matrix of one line
            hiddenLayersNeurons = new Matrix[hiddenLayersSizes.Length];  // default should be 4 - 1
            for (int i = 0; i < hiddenLayersSizes.Length; i++) {
                hiddenLayersNeurons[i] = new Matrix(1, hiddenLayersSizes[i]).SetToOne();
            }

            // synapes are an array of matrix
            // the number of line (or array of synapes sort of) is equal to the previous layer (neurons coming from)
            // the number of column (or nb of synapes in a row) is equal to the next layer (neurons going to)
            synapses = new Matrix[hiddenLayersSizes.Length + 1];
            for (int i = 0; i < hiddenLayersSizes.Length + 1; i++) {
                if (i == 0)
                    synapses[i] = new Matrix(inputLayerSize, hiddenLayersSizes[i]).SetAsSynapse();
                else if (i == hiddenLayersSizes.Length)
                    synapses[i] = new Matrix(hiddenLayersSizes[i - 1], outputLayerSize).SetAsSynapse();
                else
                    synapses[i] = new Matrix(hiddenLayersSizes[i - 2], hiddenLayersSizes[i - 1]).SetAsSynapse();
            }
        }

        public LayeredNetwork GetClone () {
            int hiddenLayers = this.hiddenLayersNeurons.Length;
            int[] hiddenLayerSizes = new int[hiddenLayers];
            for(int i=0; i<hiddenLayers; i++) {
                hiddenLayerSizes[i] = this.hiddenLayersNeurons[i].J;
            }

            LayeredNetwork clone = new LayeredNetwork(this.inputNeurons.J, this.outputNeurons.J, hiddenLayerSizes);
            clone.InsertSynapes(this.GetSynapsesCopy());

            return clone;
        }

        // this is to pass the activation function (sigmoid here) on the neuron value and on each layer
        // a layer cannot have more than one line so we don't loop through the J
        private void ProcessActivation (Matrix mat) {
            for(int j=0; j < mat.J; j++) {
                mat.Mtx[0][j] = Sigmoid(mat.Mtx[0][j]);
            }
        }

        private float Sigmoid(float t) {
            return 1f / (1 + Mathf.Exp(-(t*2-1)));
        }

        // process the input forward to get output in the network
		public float[] PingFwd(float[] sensors) {

            inputNeurons.SetLineValues(0, sensors);

            // we ping the network
            for (int i = 0; i < hiddenLayersNeurons.Length + 1; i++) {
                    if (i == 0) {
                        hiddenLayersNeurons[0] = inputNeurons.Multiply(synapses[0], true);
                        ProcessActivation(hiddenLayersNeurons[0]);
                    } else if (i == hiddenLayersNeurons.Length) {
                        outputNeurons = hiddenLayersNeurons[i - 1].Multiply(synapses[i], true);
                        ProcessActivation(outputNeurons);
                    } else {
                        hiddenLayersNeurons[i] = hiddenLayersNeurons[i - 1].Multiply(synapses[i], true);
                        ProcessActivation(hiddenLayersNeurons[i]);
                    }
                }

            return outputNeurons.GetLineValues();
        }

        public int GetInputSize () {
            return this.inputNeurons.J;
        }

        public int GetOutputSize () {
            return this.outputNeurons.J;
        }

        public int[] GetHiddenLayersSizes() {
            int[] hiddenLayerSizes = new int[hiddenLayersNeurons.Length];
            for (int i = 0; i < hiddenLayersNeurons.Length; i++) {
                hiddenLayerSizes[i] = hiddenLayersNeurons[i].J;
            }

            return hiddenLayerSizes;
        }

        public float[] GetOutputValues() {
            return this.outputNeurons.GetLineValues(0);
        }

        public Matrix[] GetSynapsesCopy () {
            Matrix[] synapsesCopy = new Matrix[synapses.Length];
            for (int i = 0; i < synapses.Length; i++) {
                synapsesCopy[i] = synapses[i].GetClone();
            }
            return synapsesCopy;
        }

        public void InsertSynapes(Matrix[] newSynapses) {

            if(synapses.Length == newSynapses.Length){
                for (int i = 0; i < synapses.Length; i++) {
                    synapses[i].SetAllValues(newSynapses[i]);
                }  
            } else {
                Debug.LogWarning("The number of synapses matrices to insert does not match the number of this network. Doing nothing.");
            }
        }
    }
}
