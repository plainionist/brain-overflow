<template>
  <div class="container">
    <div class="toolbar">
      <input type="text" placeholder="Search ..." v-model="searchQuery" @input="onSearchInput" style="flex: 1" />
      <button v-if="searchResults.length === 0" @click="onCreateNew">New</button>
      <button v-if="searchResults.length === 0" @click="onSave">Save</button>
    </div>

    <div v-if="searchResults.length > 0">
      <div v-for="item in searchResults" :key="item.snippet.id" @click="onSearchResultSelected(item)"
        class="search-result">
        {{ item.match }}
      </div>
    </div>

    <div v-else style="flex: 1">
      <textarea v-model="snippetText" class="snippet" />
    </div>

    <div v-show="showUpdates" class="updates">
      <table class="updates-table">
        <tr v-for="update in updates" :key="update.path">
          <td class="update-type">{{ update.changeType }}</td>
          <td class="update-path">{{ update.path }}</td>
        </tr>
      </table>
    </div>
  </div>
</template>

<script lang="ts">
import { defineComponent, ref } from 'vue';
import { Api } from './api'
import { listen } from '@tauri-apps/api/event';
import { getCurrentWindow, LogicalSize } from '@tauri-apps/api/window'

interface Snippet {
  id: string;
  text: string;
}

interface SearchResult {
  match: string;
  snippet: Snippet;
}

interface Change {
  changeType: string;
  path: string;
}

export default defineComponent({
  setup() {
    const searchQuery = ref<string>('');
    const snippetText = ref<string>('');
    const searchResults = ref<SearchResult[]>([]);
    let snippetId: string | null = null;
    const updates = ref<Change[]>([]);
    const showUpdates = ref(false);

    const onSearchInput = async () => {
      console.log('Searching for:', searchQuery.value);

      searchResults.value = await Api.invokePlugin<Array<SearchResult>>({
        controller: 'snippets',
        action: 'search',
        data: { text: searchQuery.value }
      }) ?? [];
    };

    const onCreateNew = () => {
      console.log('Create new item');

      snippetId = null;
      snippetText.value = '';
    };

    const onSave = async () => {
      console.log('Saving content');

      await Api.invokePlugin({
        controller: 'snippets',
        action: 'save',
        data: { id: snippetId, text: snippetText.value }
      })
    };

    const onSearchResultSelected = (result: SearchResult) => {
      snippetId = result.snippet.id;
      snippetText.value = result.snippet.text;
      searchResults.value = [];
      searchQuery.value = '';
    };

    listen<string>('store-updates', async (event) => {
      updates.value = JSON.parse(event.payload) as Change[];

      console.log(`store-updates: ${updates.value.length}`);

      showUpdates.value = true;

      const window = getCurrentWindow();
      const originalSize = await window.innerSize()
      await window.setSize(new LogicalSize(900, originalSize.height));

      setTimeout(async () => {
        showUpdates.value = false;
        updates.value = [];
        await window.setSize(originalSize);
      }, 5000);

    });

    return { searchQuery, snippetText, onSearchInput, onCreateNew, onSave, searchResults, onSearchResultSelected, updates, showUpdates };
  },
});
</script>

<style>
button {
  padding: 5px 10px;
}

body {
  margin: 0;
}

.container {
  display: flex;
  flex-direction: column;
  height: 100vh;
  box-sizing: border-box;
  padding: 10px;
  margin: 0px;
}

.toolbar {
  display: flex;
  padding-bottom: 10px;
  flex: 0 0 auto;
  gap: 10px;
}

.search-result {
  border: 1px solid;
  padding: 10px;
  margin-bottom: 10px;
  cursor: pointer;
}

.snippet {
  width: 100%;
  height: 100%;
  resize: none;
  box-sizing: border-box;
  font-size: 24px;
  font-weight: bold;
  font-family: 'Courier New', Courier, monospace;
}

.updates {
  position: absolute;
  top: 0;
  right: 0;
  width: 400px;
  height: calc(100% - 22px);
  background-color: #f5f5f5;
  border: 1px solid #ccc;
  margin: 10px;
  overflow-y: auto;
}

.updates-table {
  width: 100%;
  border-collapse: collapse;
}

.updates-table td {
  padding: 5px 10px;
}

.update-type {
  white-space: nowrap;
  text-align: left;
}

.update-path {
  text-align: left;
  width: 100%;
}
</style>
