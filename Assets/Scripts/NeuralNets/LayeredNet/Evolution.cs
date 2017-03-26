﻿using UnityEngine;
using nfs.tools;

namespace nfs.nets.layered {

	
	public static class Evolution {

		/// <summary>
        /// Creates the mutated offspring.
        /// </summary>
        /// <returns>The mutated offspring.</returns>
        /// <param name="neuralNet">Neural net.</param>
        /// <param name="mutateCoef">Mutate coef.</param>
        public static Network CreateMutatedOffspring(Network neuralNet, int mutateCoef,
													bool hiddenLayerNbMutation, float hiddenLayerNbMutationRate,
													bool hiddenNbMutation, float hiddenMbMutationRate,
													float synapsesMutationRate, float synapsesMutationRange) {

            int[] hiddenLayersSizes = neuralNet.HiddenLayersSizes;
            Matrix[] synapses = neuralNet.GetSynapsesClone();

            // TODO LATER
            // Implemenet here mutation for new input
            // have a fixe array of sensors and an array of int containing the sensor indx to read from
            // mutate this array of int

            // mutate number of hidden layers
            if(hiddenLayerNbMutation && Random.value < hiddenLayerNbMutationRate)
                hiddenLayersSizes = MutateNbOfHiddenLayer(neuralNet, hiddenLayersSizes, synapses);

            // mutated number of neurons in hidden layers
            if(hiddenNbMutation && Random.value < hiddenMbMutationRate)
                hiddenLayersSizes = MutateNbOfHiddenLayerNeurons(neuralNet, hiddenLayersSizes, synapses);

            // mutate synapses values
            synapses = MutateSynapsesValues(neuralNet, synapses, synapsesMutationRate, synapsesMutationRange);

            int[] layerSizes = new int[hiddenLayersSizes.Length + 2];
            layerSizes[0] = neuralNet.InputSize;
            layerSizes[layerSizes.Length-1] = neuralNet.OutputSize;

            for(int i=1; i<layerSizes.Length-1; i++) {
                layerSizes[i] = hiddenLayersSizes[i-1];
            }

            Network mutadedOffspring = new Network(layerSizes);
            mutadedOffspring.InsertSynapses(synapses);

            return mutadedOffspring;
        }


		/// <summary>
		/// Mutates the nb of hidden layer.
		/// </summary>
		/// <param name="neuralNet">Neural net.</param>
		/// <param name="hiddenLayersSizes">Hidden layers sizes.</param>
		/// <param name="synapses">Synapses.</param>
        private static int[] MutateNbOfHiddenLayer(Network neuralNet, int[] hiddenLayersSizes, Matrix[] synapses) {
            
			if (Random.value < 0.5f && hiddenLayersSizes.Length > 1) { // random to get positive vs negative value
				hiddenLayersSizes = RedimentionLayersNb(hiddenLayersSizes, -1);

				synapses = RedimentionLayersNb(synapses, -1);
				synapses[synapses.Length - 1] = Matrix.Redimension(synapses[synapses.Length - 1], hiddenLayersSizes[hiddenLayersSizes.Length - 1], neuralNet.OutputSize);

			} else {
				hiddenLayersSizes = RedimentionLayersNb(hiddenLayersSizes, +1);
				hiddenLayersSizes[hiddenLayersSizes.Length - 1] = neuralNet.OutputSize;

				synapses = RedimentionLayersNb(synapses, +1);
				synapses[synapses.Length - 1] = new Matrix(hiddenLayersSizes[hiddenLayersSizes.Length - 1], neuralNet.OutputSize).SetAsSynapse();
            }

			return hiddenLayersSizes;
        }

		/// <summary>
		/// Mutates the nb of hidden layer neurons.
		/// </summary>
		/// <param name="neuralNet">Neural net.</param>
		/// <param name="hiddenLayersSizes">Hidden layers sizes.</param>
		/// <param name="synapses">Synapses.</param>
        private static int[] MutateNbOfHiddenLayerNeurons(Network neuralNet, int[] hiddenLayersSizes, Matrix[] synapses) {

			int layerNb = Random.Range(0, hiddenLayersSizes.Length - 1);
			if (Random.value < 0.5f && hiddenLayersSizes[layerNb] > 1) { // random to get positive vs negative value
				hiddenLayersSizes[layerNb] -= 1;
			} else {
				hiddenLayersSizes[layerNb] += 1;
			}
			// need to use the previous synapses values here as we might be going from/to oustide of the hidden layers
			synapses[layerNb] = Matrix.Redimension(synapses[layerNb], synapses[layerNb].I, hiddenLayersSizes[layerNb]);
			synapses[layerNb+1] = Matrix.Redimension(synapses[layerNb+1], hiddenLayersSizes[layerNb], synapses[layerNb+1].J);

			return hiddenLayersSizes;
        }

		/// <summary>
		/// Mutates the synapses values.
		/// </summary>
		/// <param name="neuralNet">Neural net.</param>
		/// <param name="synapses">Synapses.</param>
        private static Matrix[] MutateSynapsesValues(Network neuralNet, Matrix[] synapses, float synapsesMutationRate, float synapsesMutationRange) {
            
			for (int n=0; n<synapses.Length; n++) {
                for (int i = 0; i < synapses[n].I; i++) {
                    for (int j=0; j < synapses[n].J; j++) {

                        if (Random.value < synapsesMutationRate) {
                            MutationType type = (MutationType)Random.Range(0, System.Enum.GetValues(typeof(MutationType)).Length-1);
                            float mutatedValue = synapses[n].GetValue(i, j);;

                            switch(type) {
                                case MutationType.additive:
                                    mutatedValue += Random.Range(-synapsesMutationRange, synapsesMutationRange);
                                    break;

                                case MutationType.multiply:
                                    mutatedValue *= Random.Range(1f - 5f *synapsesMutationRange, 1f + 5f * synapsesMutationRange);
                                    break;

                                case MutationType.reverse:
                                    mutatedValue *= -1;
                                    break;

                                case MutationType.replace:
                                    float weightRange = Matrix.StandardSynapseRange(synapses[n].J);
                                    mutatedValue = Random.Range(-weightRange, weightRange);
                                    break;

                                case MutationType.nullify:
                                    mutatedValue = 0f;
                                    break;

                                default:
                                    Debug.LogWarning("Unknown weight mutation type. Doing nothing.");
                                    break;
                            }

                            synapses[n].SetValue(i, j, mutatedValue);  
                        }
                    }
                }
            }

			return synapses;
        }

		/// <summary>
		/// Redimension an array of in for the hidden layers.
		/// </summary>
		/// <returns>The layers nb.</returns>
		/// <param name="currentLayers">Current layers.</param>
		/// <param name="sizeMod">Size mod.</param>
        public static int[] RedimentionLayersNb (int[] currentLayers, int sizeMod) {

            int[] newLayers = new int[currentLayers.Length + sizeMod];
            for (int i = 0; i < Mathf.Min(currentLayers.Length, newLayers.Length); i++) {
                newLayers[i] = currentLayers[i];
            }

            return newLayers;
        }

		/// <summary>
		/// Redimension an array of matrix for the synapses.
		/// </summary>
		/// <returns>The layers nb.</returns>
		/// <param name="currentLayers">Current layers.</param>
		/// <param name="sizeMod">Size mod.</param>
        public static Matrix[] RedimentionLayersNb (Matrix[] currentLayers, int sizeMod) {

            Matrix[] newLayers = new Matrix[currentLayers.Length + sizeMod];
            for (int i = 0; i < Mathf.Min(currentLayers.Length, newLayers.Length); i++) {
                newLayers[i] = currentLayers[i];
            }

            return newLayers;
        }

		/// <summary>
		// Compares a given neural network to a list of other and if better stores it at the correct rank.
		// Compares the network to the current generation as well as overall best network in all generations.
		/// </summary>
		/// <param name="fitnessRankings">Fitness rankings.</param>
		/// <param name="fitnessContender">Fitness contender.</param>
        public static Network[] RankFitnessContender (Network[] fitnessRankings, Network fitnessContender) {
            int last = fitnessRankings.Length-1;

            // first we take care of the first case of an empty array (no other contender yet)
            if(fitnessRankings[last] == null) {
                fitnessRankings[last] = fitnessContender;

            } else if(fitnessRankings[last] != null && fitnessRankings[last].FitnessScore < fitnessContender.FitnessScore) {
                fitnessRankings[last] = fitnessContender;
            }

            // then we go through the rest of the arrays
            if (fitnessRankings.Length > 1) { // just making sure there is  more than one network to breed (there can't be less)

                // we go from last to first in the loop
                for (int i = fitnessRankings.Length - 2; i >= 0; i--) {
                    if (fitnessRankings[i] == null) { // if the array is empty we fill it one step at a time
                        fitnessRankings[i] = fitnessContender;
                        fitnessRankings[i + 1] = null;

                    } else if(fitnessRankings[i].FitnessScore < fitnessContender.FitnessScore) {
                        Network stepDown = fitnessRankings[i];
                        fitnessRankings[i] = fitnessContender;
                        fitnessRankings[i + 1] = stepDown;

                    } else {
                        i = 0; // if the contender doesn't have a better score anymore we exit the loop
                    }
                }
            }

			return fitnessRankings;
        }

	}
}
