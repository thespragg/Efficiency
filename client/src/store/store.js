import Vue from "vue";
import Vuex from "vuex";

Vue.use(Vuex);

export const store = new Vuex.Store({
  state: {
    api: "https://localhost:44301/api"
  },
  mutations: {

  },
  getters: {
    api: state => state.api
  }
});
